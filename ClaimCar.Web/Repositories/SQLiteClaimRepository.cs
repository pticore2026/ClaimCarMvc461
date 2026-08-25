using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Web.Hosting;
using ClaimCar.Web.Models;

namespace ClaimCar.Web.Repositories
{
    public sealed class SQLiteClaimRepository : IClaimRepository
    {
        private const string Columns = "ID,MA_DON_VI,TEN_DON_VI,MA_KHU_VUC,TEN_KHU_VUC,BIEN_SO,NGAY_NHAP,NGAY_NHAP_CALL,SO_HOP_DONG,TINH_TRANG,NGAY_QUYET_DINH,NGAY_XAY_RA,NGAY_THONG_BAO,SO_HO_SO,MA_GDV,GIA_TRI_BH";
        private const string Schema = @"CREATE TABLE IF NOT EXISTS CLAIM_GENERAL (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            MA_DON_VI TEXT NOT NULL, TEN_DON_VI TEXT NOT NULL,
            MA_KHU_VUC TEXT NOT NULL, TEN_KHU_VUC TEXT NOT NULL,
            BIEN_SO TEXT NOT NULL, NGAY_NHAP TEXT NOT NULL, NGAY_NHAP_CALL TEXT NULL,
            SO_HOP_DONG TEXT NOT NULL, TINH_TRANG TEXT NOT NULL, NGAY_QUYET_DINH TEXT NULL,
            NGAY_XAY_RA TEXT NOT NULL, NGAY_THONG_BAO TEXT NOT NULL,
            SO_HO_SO TEXT NOT NULL UNIQUE, MA_GDV TEXT NULL,
            GIA_TRI_BH NUMERIC NOT NULL DEFAULT 0,
            CHECK (NGAY_THONG_BAO >= NGAY_XAY_RA));
            CREATE INDEX IF NOT EXISTS IX_CLAIM_GENERAL_SEARCH
            ON CLAIM_GENERAL(BIEN_SO, SO_HOP_DONG, TINH_TRANG);";

        private readonly string _connectionString;

        public SQLiteClaimRepository()
        {
            var setting = ConfigurationManager.ConnectionStrings["SQLiteClaimDb"];
            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                throw new ConfigurationErrorsException("Thiếu connection string SQLiteClaimDb trong Web.config.");

            var appData = HostingEnvironment.MapPath("~/App_Data") ?? AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrWhiteSpace(appData)) appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appData);
            AppDomain.CurrentDomain.SetData("DataDirectory", appData);
            _connectionString = setting.ConnectionString;
            InitializeSchema();
        }

        public IList<Claim> Search(string keyword, string status)
        {
            var claims = new List<Claim>();
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + Columns + @" FROM CLAIM_GENERAL
                    WHERE (@keyword IS NULL OR LOWER(SO_HO_SO) LIKE @pattern OR LOWER(BIEN_SO) LIKE @pattern OR LOWER(SO_HOP_DONG) LIKE @pattern)
                    AND (@status IS NULL OR TINH_TRANG=@status) ORDER BY NGAY_NHAP DESC";
                var value = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim().ToLowerInvariant();
                Add(command, "@keyword", value); Add(command, "@pattern", value == null ? null : "%" + value + "%");
                Add(command, "@status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
                using (var reader = command.ExecuteReader()) while (reader.Read()) claims.Add(MapClaim(reader));
            }
            return claims;
        }

        public Claim Get(int id)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + Columns + " FROM CLAIM_GENERAL WHERE ID=@id";
                Add(command, "@id", id);
                using (var reader = command.ExecuteReader()) return reader.Read() ? MapClaim(reader) : null;
            }
        }

        public int Insert(Claim claim)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO CLAIM_GENERAL
                    (MA_DON_VI,TEN_DON_VI,MA_KHU_VUC,TEN_KHU_VUC,BIEN_SO,NGAY_NHAP,NGAY_NHAP_CALL,SO_HOP_DONG,TINH_TRANG,NGAY_QUYET_DINH,NGAY_XAY_RA,NGAY_THONG_BAO,SO_HO_SO,MA_GDV,GIA_TRI_BH)
                    VALUES (@a,@b,@c,@d,@e,@f,@g,@h,@i,@j,@k,@l,@m,@n,@o);
                    SELECT last_insert_rowid();";
                BindClaim(command, claim);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(Claim claim)
        {
            Execute(@"UPDATE CLAIM_GENERAL SET MA_DON_VI=@a,TEN_DON_VI=@b,MA_KHU_VUC=@c,TEN_KHU_VUC=@d,
                BIEN_SO=@e,NGAY_NHAP=@f,NGAY_NHAP_CALL=@g,SO_HOP_DONG=@h,TINH_TRANG=@i,NGAY_QUYET_DINH=@j,
                NGAY_XAY_RA=@k,NGAY_THONG_BAO=@l,SO_HO_SO=@m,MA_GDV=@n,GIA_TRI_BH=@o WHERE ID=@id", command => {
                    BindClaim(command, claim); Add(command, "@id", claim.Id);
                });
        }

        public void Delete(int id) { Execute("DELETE FROM CLAIM_GENERAL WHERE ID=@id", command => Add(command, "@id", id)); }

        public bool ClaimNumberExists(string claimNumber, int? exceptId)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM CLAIM_GENERAL WHERE SO_HO_SO=@number AND (@id IS NULL OR ID<>@id)";
                Add(command, "@number", claimNumber); Add(command, "@id", exceptId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public LossPaymentViewModel GetLossPayment(int claimId) { return new DemoClaimRepository().GetLossPayment(claimId); }
        public QuoteViewModel GetQuote(int claimId) { return new DemoClaimRepository().GetQuote(claimId); }
        public void SaveLossPayment(LossPaymentViewModel model) { throw ModuleNotImplemented("Tổn thất/Chi trả"); }
        public void SaveQuote(QuoteViewModel model) { throw ModuleNotImplemented("Báo giá"); }

        private void InitializeSchema() { Execute(Schema, command => { }); }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private void Execute(string sql, Action<SQLiteCommand> bind)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql; bind(command); command.ExecuteNonQuery();
            }
        }

        private static NotSupportedException ModuleNotImplemented(string module)
        {
            return new NotSupportedException("Module " + module + " chưa triển khai persistence SQLite; quản lý hồ sơ chung đã hỗ trợ đầy đủ.");
        }

        private static void Add(SQLiteCommand command, string name, object value)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        private static void BindClaim(SQLiteCommand command, Claim claim)
        {
            Add(command, "@a", claim.ManagementUnitCode); Add(command, "@b", claim.ManagementUnitName);
            Add(command, "@c", claim.ManagementAreaCode); Add(command, "@d", claim.ManagementAreaName);
            Add(command, "@e", claim.LicensePlate); Add(command, "@f", claim.EntryDate.ToString("o"));
            Add(command, "@g", IsoDate(claim.CallEntryDate)); Add(command, "@h", claim.PolicyNumber);
            Add(command, "@i", claim.Status); Add(command, "@j", IsoDate(claim.DecisionDate));
            Add(command, "@k", claim.AccidentDate.ToString("o")); Add(command, "@l", claim.NotificationDate.ToString("o"));
            Add(command, "@m", claim.ClaimNumber); Add(command, "@n", claim.SurveyorCode); Add(command, "@o", claim.InsuredValue);
        }

        private static object IsoDate(DateTime? value) { return value.HasValue ? (object)value.Value.ToString("o") : DBNull.Value; }

        private static Claim MapClaim(SQLiteDataReader reader)
        {
            return new Claim {
                Id=Convert.ToInt32(reader["ID"]), ManagementUnitCode=reader["MA_DON_VI"].ToString(), ManagementUnitName=reader["TEN_DON_VI"].ToString(),
                ManagementAreaCode=reader["MA_KHU_VUC"].ToString(), ManagementAreaName=reader["TEN_KHU_VUC"].ToString(), LicensePlate=reader["BIEN_SO"].ToString(),
                EntryDate=Convert.ToDateTime(reader["NGAY_NHAP"]), CallEntryDate=NullableDate(reader["NGAY_NHAP_CALL"]), PolicyNumber=reader["SO_HOP_DONG"].ToString(),
                Status=reader["TINH_TRANG"].ToString(), DecisionDate=NullableDate(reader["NGAY_QUYET_DINH"]), AccidentDate=Convert.ToDateTime(reader["NGAY_XAY_RA"]),
                NotificationDate=Convert.ToDateTime(reader["NGAY_THONG_BAO"]), ClaimNumber=reader["SO_HO_SO"].ToString(), SurveyorCode=reader["MA_GDV"].ToString(),
                InsuredValue=reader["GIA_TRI_BH"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["GIA_TRI_BH"])
            };
        }

        private static DateTime? NullableDate(object value) { return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value); }
    }
}
