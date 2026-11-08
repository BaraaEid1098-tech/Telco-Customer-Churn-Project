#!/bin/bash
set -Eeuo pipefail

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
SQL_SERVER=sqlserver,1433
BACKUP_FILE=/backup/ChurnPredictionDB.bak
DATABASE=ChurnPredictionDB

sqlcmd() {
	"$SQLCMD" -S "$SQL_SERVER" -U sa -P "$MSSQL_SA_PASSWORD" -C -b "$@"
}

database_exists=$(sqlcmd -h -1 -W -Q \
	"SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'${DATABASE}') IS NULL THEN 0 ELSE 1 END;" \
	| tr -d '[:space:]')

if [[ "$database_exists" == "1" ]]; then
	echo "Database ${DATABASE} already exists; skipping restore."
	exit 0
fi

file_metadata=$(sqlcmd -h -1 -W -s "|" -Q \
	"SET NOCOUNT ON; RESTORE FILELISTONLY FROM DISK = N'${BACKUP_FILE}';" \
	| awk -F'|' '
		NF >= 3 {
			type = $3
			name = $1
			gsub(/^[[:space:]]+|[[:space:]]+$/, "", type)
			gsub(/^[[:space:]]+|[[:space:]]+$/, "", name)
			if (type == "D" || type == "L") {
				print type "|" name
			}
		}
	')

if [[ -z "$file_metadata" ]]; then
	echo "RESTORE FILELISTONLY returned no data or log files." >&2
	exit 1
fi

move_clauses=()
data_count=0
log_count=0

while IFS='|' read -r file_type logical_name; do
	[[ -n "$logical_name" ]] || continue
	escaped_name=${logical_name//\'/\'\'}

	case "$file_type" in
		D)
			((data_count += 1))
			if ((data_count == 1)); then
				target_file="${DATABASE}.mdf"
			else
				target_file="${DATABASE}_${data_count}.ndf"
			fi
			;;
		L)
			((log_count += 1))
			if ((log_count == 1)); then
				target_file="${DATABASE}_log.ldf"
			else
				target_file="${DATABASE}_log_${log_count}.ldf"
			fi
			;;
		*)
			continue
			;;
	esac

	move_clauses+=("MOVE N'${escaped_name}' TO N'/var/opt/mssql/data/${target_file}'")
done <<< "$file_metadata"

if ((data_count == 0 || log_count == 0)); then
	echo "RESTORE FILELISTONLY must include at least one data file and one log file." >&2
	exit 1
fi

printf -v move_sql '%s, ' "${move_clauses[@]}"
move_sql=${move_sql%, }

echo "Restoring ${DATABASE} from ${BACKUP_FILE}."
sqlcmd -Q \
	"RESTORE DATABASE [${DATABASE}] FROM DISK = N'${BACKUP_FILE}' WITH ${move_sql}, RECOVERY;"
echo "Database ${DATABASE} restored successfully."
