#!/usr/bin/env bash
set -euo pipefail
# Run from the repository root after building the solution.
claimcar_test_dir=$(mktemp -d /tmp/claimcar-complete-tests.XXXXXX)
cat > "$claimcar_test_dir/tests.exe.config" <<CONFIG
<configuration><connectionStrings><add name="SQLiteClaimDb" connectionString="Data Source=$claimcar_test_dir/test.db;Version=3;Foreign Keys=True;" /></connectionStrings></configuration>
CONFIG
mcs -out:"$claimcar_test_dir/tests.exe" -r:ClaimCar.Web/bin/ClaimCar.Web.dll -r:ClaimCar.Web/bin/System.Web.Mvc.dll -r:System.ComponentModel.DataAnnotations -r:System.Web tests/CompleteClaimTests.cs
MONO_PATH="$PWD/ClaimCar.Web/bin" mono "$claimcar_test_dir/tests.exe"
