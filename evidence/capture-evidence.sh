#!/usr/bin/env bash
# Capture evidence for all 12 rubric items by hitting the running API.
# Output goes to evidence/output/*.json and evidence/EVIDENCE.md (paste into Word).
#
# Usage:
#   cd labs/lab1/SE173473
#   docker compose up -d
#   ./evidence/capture-evidence.sh
#
# Requires: curl, jq

set -euo pipefail

BASE="${BASE:-http://localhost:8080}"
OUT_DIR="$(cd "$(dirname "$0")" && pwd)/output"
MD="$(cd "$(dirname "$0")" && pwd)/EVIDENCE.md"

mkdir -p "$OUT_DIR"
: > "$MD"

# Helper: capture an endpoint -> json file + markdown block
capture() {
  local id="$1"; local label="$2"; local method="${3:-GET}"; local url="$4"; local body="${5:-}"
  local file="$OUT_DIR/${id}.json"
  local status
  local hdrs="$OUT_DIR/${id}.headers"

  if [[ -n "$body" ]]; then
    status=$(curl -s -o "$file" -D "$hdrs" -w '%{http_code}' \
      -X "$method" "$BASE$url" \
      -H 'Content-Type: application/json' \
      -d "$body")
  else
    status=$(curl -s -o "$file" -D "$hdrs" -w '%{http_code}' -X "$method" "$BASE$url")
  fi

  {
    echo "## ${id} — ${label}"
    echo
    echo "**Request:** \`${method} ${url}\`"
    echo "**HTTP Status:** \`${status}\`"
    echo
    echo '```json'
    if command -v jq >/dev/null 2>&1 && [[ -s "$file" ]]; then
      jq . "$file" 2>/dev/null | head -60 || cat "$file"
    else
      head -60 "$file"
    fi
    echo '```'
    echo
  } >> "$MD"
}

echo "# PRN232 Lab 1 — Evidence Bundle (SE173473)" >> "$MD"
echo "" >> "$MD"
echo "Base URL: \`$BASE\`  Generated: \`$(date '+%Y-%m-%d %H:%M:%S')\`" >> "$MD"
echo "" >> "$MD"
echo "Stack must be running: \`docker compose up -d\`" >> "$MD"
echo "" >> "$MD"

# ── Rubric 5: RESTful naming (Swagger doc lists endpoints) ────────────────────
capture "R05-swagger-spec" "Swagger/OpenAPI document (plural, no verbs)" GET "/swagger/v1/swagger.json"

# ── Rubric 6: GET by id + 404 + related data ──────────────────────────────────
capture "R06a-getById-200-with-related" "GET student by id with related enrollments expanded" \
  GET "/api/v1/students/2?expand=enrollments"
capture "R06b-getById-404" "GET non-existent student returns 404" \
  GET "/api/v1/students/999999"

# ── Rubric 7: List capabilities (search/sort/page/fields/expand) ──────────────
capture "R07a-search"  "List with search filter" \
  GET "/api/v1/students?search=a&page=1&size=3"
capture "R07b-sort"    "List with multi-field sort (asc/desc)" \
  GET "/api/v1/students?sort=fullName,-dateOfBirth&page=1&size=5"
capture "R07c-paging"  "List with paging" \
  GET "/api/v1/students?page=2&size=5"
capture "R07d-fields"  "List with field selection" \
  GET "/api/v1/students?fields=studentId,fullName,email&page=1&size=3"
capture "R07e-expand"  "List with expand related entities" \
  GET "/api/v1/enrollments?expand=student,course&page=1&size=3"
capture "R07f-combined" "Combined search+sort+page+fields+expand" \
  GET "/api/v1/enrollments?search=Active&sort=-enrollDate&page=1&size=10&fields=enrollmentId,status&expand=student,course"

# ── Rubric 8: Pagination metadata ─────────────────────────────────────────────
capture "R08-pagination-metadata" "Response contains pagination { page, pageSize, totalItems, totalPages }" \
  GET "/api/v1/students?page=1&size=10"

# ── Rubric 9: Response envelope + HTTP status codes ───────────────────────────
capture "R09a-status-200" "200 OK with { success, message, data, errors } envelope" \
  GET "/api/v1/semesters"
capture "R09b-status-201-Created" "201 Created with Location header (see headers file)" \
  POST "/api/v1/students" \
  '{"fullName":"Evidence User","email":"evidence-'"$(date +%s)"'@lms.local","dateOfBirth":"2003-01-01"}'
capture "R09c-status-404" "404 Not Found for missing resource" \
  GET "/api/v1/courses/999999"
capture "R09d-status-400" "400 Bad Request on invalid payload (missing required field)" \
  POST "/api/v1/students" '{"email":"bad@no-name.local"}'

# ── Submission endpoint (lecturer-requested screenshot) ───────────────────────
capture "SUBMISSION-courses-id-enrollments-expand-student" \
  "Lecturer-requested: GET /courses/{id}/enrollments?expand=student" \
  GET "/api/v1/courses/1/enrollments?expand=student"

# ── HATEOAS proof (V3 bonus, not required but nice to have) ───────────────────
capture "BONUS-hateoas-links" "Response contains _links (HAL)" \
  GET "/api/v1/students/2"

# ── Counts (Rubric 3: seed verification — alternative to sqlcmd) ──────────────
echo "" >> "$MD"
echo "## R03 — DB seed counts (via list totals)" >> "$MD"
echo "" >> "$MD"
echo "| Resource | totalItems |" >> "$MD"
echo "|---|---|" >> "$MD"
for r in semesters subjects courses students enrollments; do
  total=$(curl -s "$BASE/api/v1/$r?page=1&size=1" | jq -r '.pagination.totalItems // "?"')
  echo "| $r | $total |" >> "$MD"
done
echo "" >> "$MD"
echo "**Expected:** 5 / 10 / 20 / 50 / 500" >> "$MD"

echo "" >> "$MD"
echo "## R10 — Docker stack status" >> "$MD"
echo "" >> "$MD"
echo '```' >> "$MD"
docker compose ps 2>/dev/null >> "$MD" || echo "(docker compose not available in this shell)" >> "$MD"
echo '```' >> "$MD"

echo "Done. Markdown -> $MD"
echo "Raw JSON   -> $OUT_DIR/"
