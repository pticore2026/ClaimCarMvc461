using System;
using System.Collections.Generic;
using System.Configuration;
using ClaimCar.Web.Models;
using MySql.Data.MySqlClient;

namespace ClaimCar.Web.Repositories
{
    public class MySqlClaimRepository : IClaimRepository
    {
        private const string Columns = "ID,MA_DON_VI,TEN_DON_VI,MA_KHU_VUC,TEN_KHU_VUC,BIEN_SO,NGAY_NHAP,NGAY_NHAP_CALL,SO_HOP_DONG,TINH_TRANG,NGAY_QUYET_DINH,NGAY_XAY_RA,NGAY_THONG_BAO,SO_HO_SO,MA_GDV,GIA_TRI_BH";
        private readonly string _connectionString;

        public MySqlClaimRepository()
        {
            var setting = ConfigurationManager.ConnectionStrings["MySqlClaimDb"];
            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                throw new ConfigurationErrorsException("Thiếu connection string MySqlClaimDb trong Web.config.");
            _connectionString = setting.ConnectionString;
        }

        public IList<Claim> Search(string keyword, string status)
        {
            var result = new List<Claim>();
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + Columns + @" FROM CLAIM_GENERAL
                    WHERE (@keyword IS NULL OR LOWER(SO_HO_SO) LIKE @pattern OR LOWER(BIEN_SO) LIKE @pattern OR LOWER(SO_HOP_DONG) LIKE @pattern)
                      AND (@status IS NULL OR TINH_TRANG = @status)
                    ORDER BY NGAY_NHAP DESC";
                var normalizedKeyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim().ToLowerInvariant();
                Add(command, "@keyword", normalizedKeyword);
                Add(command, "@pattern", normalizedKeyword == null ? null : "%" + normalizedKeyword + "%");
                Add(command, "@status", string.IsNullOrWhiteSpace(status) ? null : status.Trim());
                connection.Open();
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) result.Add(MapClaim(reader));
            }
            return result;
        }

        public Claim Get(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + Columns + " FROM CLAIM_GENERAL WHERE ID=@id";
                Add(command, "@id", id);
                connection.Open();
                using (var reader = command.ExecuteReader()) return reader.Read() ? MapClaim(reader) : null;
            }
        }

        public int Insert(Claim claim)
        {
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO CLAIM_GENERAL
                    (MA_DON_VI,TEN_DON_VI,MA_KHU_VUC,TEN_KHU_VUC,BIEN_SO,NGAY_NHAP,NGAY_NHAP_CALL,SO_HOP_DONG,TINH_TRANG,NGAY_QUYET_DINH,NGAY_XAY_RA,NGAY_THONG_BAO,SO_HO_SO,MA_GDV,GIA_TRI_BH)
                    VALUES (@a,@b,@c,@d,@e,@f,@g,@h,@i,@j,@k,@l,@m,@n,@o);";
                BindClaim(command, claim);
                connection.Open();
                command.ExecuteNonQuery();
                return checked((int)command.LastInsertedId);
            }
        }

        public void Update(Claim claim)
        {
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE CLAIM_GENERAL SET
                    MA_DON_VI=@a,TEN_DON_VI=@b,MA_KHU_VUC=@c,TEN_KHU_VUC=@d,BIEN_SO=@e,NGAY_NHAP=@f,NGAY_NHAP_CALL=@g,
                    SO_HOP_DONG=@h,TINH_TRANG=@i,NGAY_QUYET_DINH=@j,NGAY_XAY_RA=@k,NGAY_THONG_BAO=@l,SO_HO_SO=@m,MA_GDV=@n,GIA_TRI_BH=@o
                    WHERE ID=@id";
                BindClaim(command, claim);
                Add(command, "@id", claim.Id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            Execute("DELETE FROM CLAIM_GENERAL WHERE ID=@id", command => Add(command, "@id", id));
        }

        public bool ClaimNumberExists(string claimNumber, int? exceptId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM CLAIM_GENERAL WHERE SO_HO_SO=@number AND (@id IS NULL OR ID<>@id)";
                Add(command, "@number", claimNumber);
                Add(command, "@id", exceptId);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public VehiclePolicy GetVehiclePolicy(string policyNumber)
        {
            if(string.IsNullOrWhiteSpace(policyNumber)) return null;
            using(var connection=new MySqlConnection(_connectionString))
            using(var command=connection.CreateCommand())
            {
                command.CommandText=@"SELECT ID,SO_HOP_DONG,SO_DON_BAO_HIEM,BIEN_SO,SO_KHUNG,SO_MAY,NHAN_HIEU,DONG_XE,
                    HIEU_LUC_TU,HIEU_LUC_DEN,GIA_TRI_XE,SO_TIEN_BAO_HIEM,TRANG_THAI
                    FROM VEHICLE_POLICY WHERE SO_HOP_DONG=@number";
                Add(command,"@number",policyNumber.Trim()); connection.Open();
                using(var r=command.ExecuteReader())return r.Read()?new VehiclePolicy{Id=Convert.ToInt32(r["ID"]),PolicyNumber=r["SO_HOP_DONG"].ToString(),CertificateNumber=r["SO_DON_BAO_HIEM"].ToString(),LicensePlate=r["BIEN_SO"].ToString(),ChassisNumber=r["SO_KHUNG"].ToString(),EngineNumber=r["SO_MAY"].ToString(),Brand=r["NHAN_HIEU"].ToString(),Model=r["DONG_XE"].ToString(),EffectiveFrom=Convert.ToDateTime(r["HIEU_LUC_TU"]),EffectiveTo=Convert.ToDateTime(r["HIEU_LUC_DEN"]),VehicleValue=Convert.ToDecimal(r["GIA_TRI_XE"]),InsuredAmount=Convert.ToDecimal(r["SO_TIEN_BAO_HIEM"]),Status=r["TRANG_THAI"].ToString()}:null;
            }
        }

        public LossPaymentViewModel GetLossPayment(int claimId) { return new DemoClaimRepository().GetLossPayment(claimId); }
        public QuoteViewModel GetQuote(int claimId) { return new DemoClaimRepository().GetQuote(claimId); }
        public void SaveLossPayment(LossPaymentViewModel model) { throw ModuleNotImplemented("Tổn thất/Chi trả"); }
        public void SaveQuote(QuoteViewModel model) { throw ModuleNotImplemented("Báo giá"); }

        private void Execute(string sql, Action<MySqlCommand> bind)
        {
            using (var connection = new MySqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                bind(command);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static NotSupportedException ModuleNotImplemented(string module)
        {
            return new NotSupportedException("Module " + module + " chưa triển khai persistence MySQL; quản lý hồ sơ chung đã hỗ trợ đầy đủ.");
        }

        private static void Add(MySqlCommand command, string name, object value)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        private static void BindClaim(MySqlCommand command, Claim claim)
        {
            Add(command, "@a", claim.ManagementUnitCode); Add(command, "@b", claim.ManagementUnitName);
            Add(command, "@c", claim.ManagementAreaCode); Add(command, "@d", claim.ManagementAreaName);
            Add(command, "@e", claim.LicensePlate); Add(command, "@f", claim.EntryDate);
            Add(command, "@g", claim.CallEntryDate); Add(command, "@h", claim.PolicyNumber);
            Add(command, "@i", claim.Status); Add(command, "@j", claim.DecisionDate);
            Add(command, "@k", claim.AccidentDate); Add(command, "@l", claim.NotificationDate);
            Add(command, "@m", claim.ClaimNumber); Add(command, "@n", claim.SurveyorCode);
            Add(command, "@o", claim.InsuredValue);
        }

        private static Claim MapClaim(MySqlDataReader reader)
        {
            return new Claim {
                Id = Convert.ToInt32(reader["ID"]), ManagementUnitCode = reader["MA_DON_VI"].ToString(),
                ManagementUnitName = reader["TEN_DON_VI"].ToString(), ManagementAreaCode = reader["MA_KHU_VUC"].ToString(),
                ManagementAreaName = reader["TEN_KHU_VUC"].ToString(), LicensePlate = reader["BIEN_SO"].ToString(),
                EntryDate = Convert.ToDateTime(reader["NGAY_NHAP"]), CallEntryDate = NullableDate(reader["NGAY_NHAP_CALL"]),
                PolicyNumber = reader["SO_HOP_DONG"].ToString(), Status = reader["TINH_TRANG"].ToString(),
                DecisionDate = NullableDate(reader["NGAY_QUYET_DINH"]), AccidentDate = Convert.ToDateTime(reader["NGAY_XAY_RA"]),
                NotificationDate = Convert.ToDateTime(reader["NGAY_THONG_BAO"]), ClaimNumber = reader["SO_HO_SO"].ToString(),
                SurveyorCode = reader["MA_GDV"].ToString(), InsuredValue = reader["GIA_TRI_BH"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["GIA_TRI_BH"])
            };
        }

        private static DateTime? NullableDate(object value) { return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value); }
    }
}
