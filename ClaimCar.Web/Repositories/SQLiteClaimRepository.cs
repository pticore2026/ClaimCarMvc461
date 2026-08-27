using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using ClaimCar.Web.Models;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;

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
            ON CLAIM_GENERAL(BIEN_SO, SO_HOP_DONG, TINH_TRANG);
            CREATE TABLE IF NOT EXISTS VEHICLE_POLICY (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                SO_HOP_DONG TEXT NOT NULL UNIQUE,
                SO_DON_BAO_HIEM TEXT NOT NULL UNIQUE,
                MA_DON_VI TEXT NOT NULL,
                NGAY_CAP_DON TEXT NOT NULL,
                MA_KHACH_HANG TEXT,
                TEN_CHU_XE TEXT NOT NULL,
                LOAI_KHACH_HANG TEXT NOT NULL DEFAULT 'CA_NHAN',
                SO_GIAY_TO TEXT NOT NULL,
                DIEN_THOAI TEXT,
                EMAIL TEXT,
                DIA_CHI TEXT,
                BIEN_SO TEXT NOT NULL,
                SO_KHUNG TEXT NOT NULL UNIQUE,
                SO_MAY TEXT NOT NULL,
                NHAN_HIEU TEXT NOT NULL,
                DONG_XE TEXT,
                NAM_SAN_XUAT INTEGER,
                MUC_DICH_SU_DUNG TEXT,
                SO_CHO INTEGER,
                TRONG_TAI NUMERIC,
                HIEU_LUC_TU TEXT NOT NULL,
                HIEU_LUC_DEN TEXT NOT NULL,
                PHAM_VI_BAO_HIEM TEXT NOT NULL,
                NGOAI_TE TEXT NOT NULL DEFAULT 'VND',
                GIA_TRI_XE NUMERIC NOT NULL DEFAULT 0,
                SO_TIEN_BAO_HIEM NUMERIC NOT NULL DEFAULT 0,
                PHI_TRUOC_THUE NUMERIC NOT NULL DEFAULT 0,
                THUE_GTGT NUMERIC NOT NULL DEFAULT 0,
                TONG_PHI NUMERIC NOT NULL DEFAULT 0,
                MUC_KHAU_TRU NUMERIC NOT NULL DEFAULT 0,
                KENH_KHAI_THAC TEXT,
                MA_DAI_LY TEXT,
                CAN_BO_CAP_DON TEXT,
                TRANG_THAI TEXT NOT NULL DEFAULT 'NHAP',
                GHI_CHU TEXT,
                NGAY_TAO TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                NGAY_CAP_NHAT TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CHECK (HIEU_LUC_DEN > HIEU_LUC_TU),
                CHECK (NAM_SAN_XUAT IS NULL OR NAM_SAN_XUAT BETWEEN 1900 AND 2100),
                CHECK (GIA_TRI_XE >= 0 AND SO_TIEN_BAO_HIEM >= 0 AND TONG_PHI >= 0),
                CHECK (TRANG_THAI IN ('NHAP','CHO_DUYET','DA_CAP','HUY','HET_HIEU_LUC')));
            CREATE INDEX IF NOT EXISTS IX_VEHICLE_POLICY_LOOKUP
            ON VEHICLE_POLICY(BIEN_SO, TEN_CHU_XE, TRANG_THAI, HIEU_LUC_DEN);
            CREATE TABLE IF NOT EXISTS CLAIM_LOSS_PAYMENT (
                CLAIM_ID INTEGER PRIMARY KEY REFERENCES CLAIM_GENERAL(ID) ON DELETE CASCADE,
                MA_NGUYEN_NHAN TEXT NOT NULL, MA_HANH_VI TEXT, MA_KHU_VUC TEXT NOT NULL, MA_SU_KIEN TEXT NOT NULL,
                CV_TBTN_YCB TEXT, GIA_TRI_XE_GCN NUMERIC, DIEN_BIEN TEXT NOT NULL,
                MO_TA_NGUYEN_NHAN TEXT NOT NULL, MO_TA_HAU_QUA TEXT NOT NULL,
                MA_GARA TEXT, TEN_GARA TEXT, PHONE_GARA TEXT, EMAIL_GARA TEXT,
                THANH_TOAN_QUA_GARA INTEGER NOT NULL DEFAULT 0, DOI_QUY_HIEP_HOI INTEGER NOT NULL DEFAULT 0);
            CREATE TABLE IF NOT EXISTS CLAIM_COVERAGE (
                ID INTEGER PRIMARY KEY AUTOINCREMENT, CLAIM_ID INTEGER NOT NULL REFERENCES CLAIM_GENERAL(ID) ON DELETE CASCADE,
                LOAI_HINH TEXT, NGOAI_TE TEXT, TIEN_BAO_HIEM NUMERIC, TIEN_TT NUMERIC, KHAU_TRU NUMERIC, TIEN_BOI_THUONG NUMERIC, THUE NUMERIC);
            CREATE TABLE IF NOT EXISTS CLAIM_BENEFICIARY (
                ID INTEGER PRIMARY KEY AUTOINCREMENT, CLAIM_ID INTEGER NOT NULL REFERENCES CLAIM_GENERAL(ID) ON DELETE CASCADE, MA TEXT, TEN TEXT);
            CREATE TABLE IF NOT EXISTS CLAIM_THIRD_PARTY (
                ID INTEGER PRIMARY KEY AUTOINCREMENT, CLAIM_ID INTEGER NOT NULL REFERENCES CLAIM_GENERAL(ID) ON DELETE CASCADE, TEN TEXT, NGOAI_TE TEXT, SO_TIEN NUMERIC);
            CREATE TABLE IF NOT EXISTS CLAIM_QUOTE (
                CLAIM_ID INTEGER PRIMARY KEY REFERENCES CLAIM_GENERAL(ID) ON DELETE CASCADE,
                KIEU_DUYET TEXT, GIA_TRI_THUC_TE NUMERIC, NGAY_TRINH TEXT, LY_DO_GIAM_TRU TEXT,
                TONG_THAY_THE NUMERIC, TONG_THAY_THE_DB NUMERIC, TONG_SUA_CHUA NUMERIC, TONG_SON NUMERIC, TONG_CONG NUMERIC, TONG_CAU_KEO NUMERIC,
                GG_THAY_THE NUMERIC, GG_SUA_CHUA NUMERIC, GG_SON NUMERIC, KHAU_HAO_THAY_THE NUMERIC, KHAU_HAO_DB NUMERIC,
                TL_GIA_TRI_THAM_GIA NUMERIC, TL_PHI_THAM_GIA NUMERIC, SO_VU_KHAU_TRU INTEGER, MUC_KHAU_TRU NUMERIC,
                GIAM_TRU_BT NUMERIC, CHIA_SE_RUI_RO NUMERIC, KHACH_HANG_THANH_TOAN NUMERIC, TONG_DUYET_GIA NUMERIC, CHECKER TEXT);
            CREATE TABLE IF NOT EXISTS CLAIM_QUOTE_ITEM (
                ID INTEGER PRIMARY KEY AUTOINCREMENT, CLAIM_ID INTEGER NOT NULL REFERENCES CLAIM_GENERAL(ID) ON DELETE CASCADE,
                TEN_PHU_TUNG TEXT, SO_LUONG INTEGER, PHUONG_AN TEXT, LOAI_PT TEXT, GIA_PT NUMERIC, SON NUMERIC, CONG NUMERIC);";

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

        public VehiclePolicy GetVehiclePolicy(string policyNumber)
        {
            if(string.IsNullOrWhiteSpace(policyNumber)) return null;
            using(var connection=OpenConnection())
            using(var command=connection.CreateCommand())
            {
                command.CommandText=@"SELECT ID,SO_HOP_DONG,SO_DON_BAO_HIEM,MA_DON_VI,NGAY_CAP_DON,MA_KHACH_HANG,TEN_CHU_XE,LOAI_KHACH_HANG,SO_GIAY_TO,DIEN_THOAI,EMAIL,DIA_CHI,
                    BIEN_SO,SO_KHUNG,SO_MAY,NHAN_HIEU,DONG_XE,NAM_SAN_XUAT,MUC_DICH_SU_DUNG,SO_CHO,HIEU_LUC_TU,HIEU_LUC_DEN,PHAM_VI_BAO_HIEM,NGOAI_TE,
                    GIA_TRI_XE,SO_TIEN_BAO_HIEM,PHI_TRUOC_THUE,THUE_GTGT,TONG_PHI,MUC_KHAU_TRU,KENH_KHAI_THAC,CAN_BO_CAP_DON,TRANG_THAI,GHI_CHU
                    FROM VEHICLE_POLICY WHERE SO_HOP_DONG=@number";
                Add(command,"@number",policyNumber.Trim());
                using(var reader=command.ExecuteReader()) return reader.Read()?MapPolicy(reader):null;
            }
        }

        public LossPaymentViewModel GetLossPayment(int claimId)
        {
            var model = new LossPaymentViewModel { ClaimId=claimId, Coverages=new List<CoverageLine>(), OtherBeneficiaries=new List<BeneficiaryLine>(), ThirdParties=new List<ThirdPartyLine>() };
            using (var connection = OpenConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM CLAIM_LOSS_PAYMENT WHERE CLAIM_ID=@id"; Add(command, "@id", claimId);
                    using (var r=command.ExecuteReader()) if (r.Read()) { model.CauseCode=Text(r,"MA_NGUYEN_NHAN"); model.BehaviorCode=Text(r,"MA_HANH_VI"); model.AreaCode=Text(r,"MA_KHU_VUC"); model.EventCode=Text(r,"MA_SU_KIEN"); model.TbtnYcbReference=Text(r,"CV_TBTN_YCB"); model.VehicleCertificateValue=Decimal(r,"GIA_TRI_XE_GCN"); model.AccidentDescription=Text(r,"DIEN_BIEN"); model.CauseDescription=Text(r,"MO_TA_NGUYEN_NHAN"); model.ConsequenceDescription=Text(r,"MO_TA_HAU_QUA"); model.GarageCode=Text(r,"MA_GARA"); model.GarageName=Text(r,"TEN_GARA"); model.GaragePhone=Text(r,"PHONE_GARA"); model.GarageEmail=Text(r,"EMAIL_GARA"); model.PayThroughGarage=Bool(r,"THANH_TOAN_QUA_GARA"); model.AssociationFund=Bool(r,"DOI_QUY_HIEP_HOI"); }
                }
                ReadRows(connection,"SELECT * FROM CLAIM_COVERAGE WHERE CLAIM_ID=@id ORDER BY ID",claimId,r=>model.Coverages.Add(new CoverageLine{Id=Int(r,"ID"),CoverageCode=Text(r,"LOAI_HINH"),Currency=Text(r,"NGOAI_TE"),InsuranceAmount=Decimal(r,"TIEN_BAO_HIEM"),LossAmount=Decimal(r,"TIEN_TT"),Deductible=Decimal(r,"KHAU_TRU"),CompensationAmount=Decimal(r,"TIEN_BOI_THUONG"),TaxAmount=Decimal(r,"THUE")}));
                ReadRows(connection,"SELECT * FROM CLAIM_BENEFICIARY WHERE CLAIM_ID=@id ORDER BY ID",claimId,r=>model.OtherBeneficiaries.Add(new BeneficiaryLine{Id=Int(r,"ID"),Code=Text(r,"MA"),Name=Text(r,"TEN")}));
                ReadRows(connection,"SELECT * FROM CLAIM_THIRD_PARTY WHERE CLAIM_ID=@id ORDER BY ID",claimId,r=>model.ThirdParties.Add(new ThirdPartyLine{Id=Int(r,"ID"),Name=Text(r,"TEN"),Currency=Text(r,"NGOAI_TE"),Amount=Decimal(r,"SO_TIEN")}));
            }
            return model;
        }

        public void SaveLossPayment(LossPaymentViewModel model)
        {
            using(var connection=OpenConnection()) using(var transaction=connection.BeginTransaction())
            {
                Execute(connection,transaction,"INSERT OR REPLACE INTO CLAIM_LOSS_PAYMENT VALUES(@id,@a,@b,@c,@d,@e,@f,@g,@h,@i,@j,@k,@l,@m,@n,@o)",c=>{Add(c,"@id",model.ClaimId);Add(c,"@a",model.CauseCode);Add(c,"@b",model.BehaviorCode);Add(c,"@c",model.AreaCode);Add(c,"@d",model.EventCode);Add(c,"@e",model.TbtnYcbReference);Add(c,"@f",model.VehicleCertificateValue);Add(c,"@g",model.AccidentDescription);Add(c,"@h",model.CauseDescription);Add(c,"@i",model.ConsequenceDescription);Add(c,"@j",model.GarageCode);Add(c,"@k",model.GarageName);Add(c,"@l",model.GaragePhone);Add(c,"@m",model.GarageEmail);Add(c,"@n",model.PayThroughGarage?1:0);Add(c,"@o",model.AssociationFund?1:0);});
                ReplaceChildren(connection,transaction,"CLAIM_COVERAGE",model.ClaimId,model.Coverages,(c,x)=>{c.CommandText="INSERT INTO CLAIM_COVERAGE(CLAIM_ID,LOAI_HINH,NGOAI_TE,TIEN_BAO_HIEM,TIEN_TT,KHAU_TRU,TIEN_BOI_THUONG,THUE) VALUES(@id,@a,@b,@c,@d,@e,@f,@g)";Add(c,"@a",x.CoverageCode);Add(c,"@b",x.Currency);Add(c,"@c",x.InsuranceAmount);Add(c,"@d",x.LossAmount);Add(c,"@e",x.Deductible);Add(c,"@f",x.CompensationAmount);Add(c,"@g",x.TaxAmount);});
                ReplaceChildren(connection,transaction,"CLAIM_BENEFICIARY",model.ClaimId,model.OtherBeneficiaries,(c,x)=>{c.CommandText="INSERT INTO CLAIM_BENEFICIARY(CLAIM_ID,MA,TEN) VALUES(@id,@a,@b)";Add(c,"@a",x.Code);Add(c,"@b",x.Name);});
                ReplaceChildren(connection,transaction,"CLAIM_THIRD_PARTY",model.ClaimId,model.ThirdParties,(c,x)=>{c.CommandText="INSERT INTO CLAIM_THIRD_PARTY(CLAIM_ID,TEN,NGOAI_TE,SO_TIEN) VALUES(@id,@a,@b,@c)";Add(c,"@a",x.Name);Add(c,"@b",x.Currency);Add(c,"@c",x.Amount);});
                transaction.Commit();
            }
        }

        public QuoteViewModel GetQuote(int claimId)
        {
            var model=new QuoteViewModel{ClaimId=claimId,Items=new List<QuoteItem>()};
            using(var connection=OpenConnection())
            {
                using(var c=connection.CreateCommand()){c.CommandText="SELECT * FROM CLAIM_QUOTE WHERE CLAIM_ID=@id";Add(c,"@id",claimId);using(var r=c.ExecuteReader())if(r.Read()){model.ApprovalType=Text(r,"KIEU_DUYET");model.ActualValue=Decimal(r,"GIA_TRI_THUC_TE");model.SubmitDate=NullableDate(r["NGAY_TRINH"]);model.ReductionReason=Text(r,"LY_DO_GIAM_TRU");model.ReplacementTotal=Decimal(r,"TONG_THAY_THE");model.SpecialReplacementTotal=Decimal(r,"TONG_THAY_THE_DB");model.RepairTotal=Decimal(r,"TONG_SUA_CHUA");model.PaintTotal=Decimal(r,"TONG_SON");model.LaborTotal=Decimal(r,"TONG_CONG");model.TowingTotal=Decimal(r,"TONG_CAU_KEO");model.ReplacementDiscountPercent=Decimal(r,"GG_THAY_THE");model.RepairDiscountPercent=Decimal(r,"GG_SUA_CHUA");model.PaintDiscountPercent=Decimal(r,"GG_SON");model.ReplacementDepreciationPercent=Decimal(r,"KHAU_HAO_THAY_THE");model.SpecialDepreciationPercent=Decimal(r,"KHAU_HAO_DB");model.ParticipationValuePercent=Decimal(r,"TL_GIA_TRI_THAM_GIA");model.ParticipationFeePercent=Decimal(r,"TL_PHI_THAM_GIA");model.DeductibleCases=Int(r,"SO_VU_KHAU_TRU");model.DeductibleAmount=Decimal(r,"MUC_KHAU_TRU");model.CompensationReductionPercent=Decimal(r,"GIAM_TRU_BT");model.RiskSharingPercent=Decimal(r,"CHIA_SE_RUI_RO");model.CustomerPaymentTotal=Decimal(r,"KHACH_HANG_THANH_TOAN");model.ApprovedTotal=Decimal(r,"TONG_DUYET_GIA");model.Checker=Text(r,"CHECKER");}}
                ReadRows(connection,"SELECT * FROM CLAIM_QUOTE_ITEM WHERE CLAIM_ID=@id ORDER BY ID",claimId,r=>model.Items.Add(new QuoteItem{Id=Int(r,"ID"),PartName=Text(r,"TEN_PHU_TUNG"),Quantity=Int(r,"SO_LUONG"),Proposal=Text(r,"PHUONG_AN"),PartType=Text(r,"LOAI_PT"),PartPrice=Decimal(r,"GIA_PT"),PaintCost=Decimal(r,"SON"),LaborCost=Decimal(r,"CONG")}));
            }
            return model;
        }

        public void SaveQuote(QuoteViewModel m)
        {
            using(var connection=OpenConnection())using(var transaction=connection.BeginTransaction())
            {
                Execute(connection,transaction,"INSERT OR REPLACE INTO CLAIM_QUOTE VALUES(@id,@a,@b,@c,@d,@e,@f,@g,@h,@i,@j,@k,@l,@m,@n,@o,@p,@q,@r,@s,@t,@u,@v,@w,@x)",c=>{Add(c,"@id",m.ClaimId);Add(c,"@a",m.ApprovalType);Add(c,"@b",m.ActualValue);Add(c,"@c",IsoDate(m.SubmitDate));Add(c,"@d",m.ReductionReason);Add(c,"@e",m.ReplacementTotal);Add(c,"@f",m.SpecialReplacementTotal);Add(c,"@g",m.RepairTotal);Add(c,"@h",m.PaintTotal);Add(c,"@i",m.LaborTotal);Add(c,"@j",m.TowingTotal);Add(c,"@k",m.ReplacementDiscountPercent);Add(c,"@l",m.RepairDiscountPercent);Add(c,"@m",m.PaintDiscountPercent);Add(c,"@n",m.ReplacementDepreciationPercent);Add(c,"@o",m.SpecialDepreciationPercent);Add(c,"@p",m.ParticipationValuePercent);Add(c,"@q",m.ParticipationFeePercent);Add(c,"@r",m.DeductibleCases);Add(c,"@s",m.DeductibleAmount);Add(c,"@t",m.CompensationReductionPercent);Add(c,"@u",m.RiskSharingPercent);Add(c,"@v",m.CustomerPaymentTotal);Add(c,"@w",m.ApprovedTotal);Add(c,"@x",m.Checker);});
                ReplaceChildren(connection,transaction,"CLAIM_QUOTE_ITEM",m.ClaimId,m.Items,(c,x)=>{c.CommandText="INSERT INTO CLAIM_QUOTE_ITEM(CLAIM_ID,TEN_PHU_TUNG,SO_LUONG,PHUONG_AN,LOAI_PT,GIA_PT,SON,CONG) VALUES(@id,@a,@b,@c,@d,@e,@f,@g)";Add(c,"@a",x.PartName);Add(c,"@b",x.Quantity);Add(c,"@c",x.Proposal);Add(c,"@d",x.PartType);Add(c,"@e",x.PartPrice);Add(c,"@f",x.PaintCost);Add(c,"@g",x.LaborCost);});
                transaction.Commit();
            }
        }

        private static void ReadRows(SQLiteConnection connection, string sql, int claimId, Action<SQLiteDataReader> read)
        {
            using(var command=connection.CreateCommand())
            {
                command.CommandText=sql; Add(command,"@id",claimId);
                using(var reader=command.ExecuteReader()) while(reader.Read()) read(reader);
            }
        }

        private static void Execute(SQLiteConnection connection, SQLiteTransaction transaction, string sql, Action<SQLiteCommand> bind)
        {
            using(var command=connection.CreateCommand())
            {
                command.Transaction=transaction; command.CommandText=sql; bind(command); command.ExecuteNonQuery();
            }
        }

        private static void ReplaceChildren<T>(SQLiteConnection connection, SQLiteTransaction transaction, string table, int claimId, IList<T> rows, Action<SQLiteCommand,T> bind)
        {
            Execute(connection,transaction,"DELETE FROM "+table+" WHERE CLAIM_ID=@id",c=>Add(c,"@id",claimId));
            if(rows==null) return;
            foreach(var row in rows) using(var command=connection.CreateCommand())
            {
                command.Transaction=transaction; Add(command,"@id",claimId); bind(command,row); command.ExecuteNonQuery();
            }
        }

        private static string Text(SQLiteDataReader reader,string name) { return reader[name]==DBNull.Value?null:reader[name].ToString(); }
        private static decimal Decimal(SQLiteDataReader reader,string name) { return reader[name]==DBNull.Value?0:Convert.ToDecimal(reader[name]); }
        private static int Int(SQLiteDataReader reader,string name) { return reader[name]==DBNull.Value?0:Convert.ToInt32(reader[name]); }
        private static bool Bool(SQLiteDataReader reader,string name) { return reader[name]!=DBNull.Value && Convert.ToInt32(reader[name])!=0; }

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

        private static VehiclePolicy MapPolicy(SQLiteDataReader r)
        {
            return new VehiclePolicy{Id=Int(r,"ID"),PolicyNumber=Text(r,"SO_HOP_DONG"),CertificateNumber=Text(r,"SO_DON_BAO_HIEM"),UnitCode=Text(r,"MA_DON_VI"),IssueDate=Convert.ToDateTime(r["NGAY_CAP_DON"]),CustomerCode=Text(r,"MA_KHACH_HANG"),OwnerName=Text(r,"TEN_CHU_XE"),CustomerType=Text(r,"LOAI_KHACH_HANG"),IdentityNumber=Text(r,"SO_GIAY_TO"),Phone=Text(r,"DIEN_THOAI"),Email=Text(r,"EMAIL"),Address=Text(r,"DIA_CHI"),LicensePlate=Text(r,"BIEN_SO"),ChassisNumber=Text(r,"SO_KHUNG"),EngineNumber=Text(r,"SO_MAY"),Brand=Text(r,"NHAN_HIEU"),Model=Text(r,"DONG_XE"),ManufactureYear=r["NAM_SAN_XUAT"]==DBNull.Value?(int?)null:Convert.ToInt32(r["NAM_SAN_XUAT"]),UsagePurpose=Text(r,"MUC_DICH_SU_DUNG"),Seats=r["SO_CHO"]==DBNull.Value?(int?)null:Convert.ToInt32(r["SO_CHO"]),EffectiveFrom=Convert.ToDateTime(r["HIEU_LUC_TU"]),EffectiveTo=Convert.ToDateTime(r["HIEU_LUC_DEN"]),CoverageScope=Text(r,"PHAM_VI_BAO_HIEM"),Currency=Text(r,"NGOAI_TE"),VehicleValue=Decimal(r,"GIA_TRI_XE"),InsuredAmount=Decimal(r,"SO_TIEN_BAO_HIEM"),PremiumBeforeTax=Decimal(r,"PHI_TRUOC_THUE"),VatAmount=Decimal(r,"THUE_GTGT"),TotalPremium=Decimal(r,"TONG_PHI"),Deductible=Decimal(r,"MUC_KHAU_TRU"),DistributionChannel=Text(r,"KENH_KHAI_THAC"),IssuedBy=Text(r,"CAN_BO_CAP_DON"),Status=Text(r,"TRANG_THAI"),Notes=Text(r,"GHI_CHU")};
        }
    }
}
