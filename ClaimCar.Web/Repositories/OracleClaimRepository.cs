using System;
using System.Collections.Generic;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using ClaimCar.Web.Models;
namespace ClaimCar.Web.Repositories
{
    public class OracleClaimRepository : IClaimRepository
    {
        private readonly string _cs = ConfigurationManager.ConnectionStrings["OracleClaimDb"].ConnectionString;
        public IList<Claim> Search(string keyword,string status)
        {
            var result=new List<Claim>();
            using(var cn=new OracleConnection(_cs)) using(var cmd=cn.CreateCommand())
            {
                cmd.CommandText=@"SELECT ID,MA_DON_VI,TEN_DON_VI,MA_KHU_VUC,TEN_KHU_VUC,BIEN_SO,NGAY_NHAP,NGAY_NHAP_CALL,SO_HOP_DONG,TINH_TRANG,NGAY_QUYET_DINH,NGAY_XAY_RA,NGAY_THONG_BAO,SO_HO_SO,MA_GDV,GIA_TRI_BH FROM CLAIM_GENERAL WHERE (:kw IS NULL OR LOWER(SO_HO_SO) LIKE :kw2 OR LOWER(BIEN_SO) LIKE :kw3 OR LOWER(SO_HOP_DONG) LIKE :kw4) AND (:st IS NULL OR TINH_TRANG=:st2) ORDER BY NGAY_NHAP DESC";
                Add(cmd,"kw",string.IsNullOrWhiteSpace(keyword)?null:keyword.ToLowerInvariant()); Add(cmd,"kw2",string.IsNullOrWhiteSpace(keyword)?null:"%"+keyword.ToLowerInvariant()+"%"); Add(cmd,"kw3",string.IsNullOrWhiteSpace(keyword)?null:"%"+keyword.ToLowerInvariant()+"%"); Add(cmd,"kw4",string.IsNullOrWhiteSpace(keyword)?null:"%"+keyword.ToLowerInvariant()+"%"); Add(cmd,"st",string.IsNullOrWhiteSpace(status)?null:status); Add(cmd,"st2",string.IsNullOrWhiteSpace(status)?null:status);
                cn.Open(); using(var rd=cmd.ExecuteReader()) while(rd.Read()) result.Add(MapClaim(rd));
            } return result;
        }
        public Claim Get(int id){using(var cn=new OracleConnection(_cs))using(var cmd=cn.CreateCommand()){cmd.CommandText="SELECT ID,MA_DON_VI,TEN_DON_VI,MA_KHU_VUC,TEN_KHU_VUC,BIEN_SO,NGAY_NHAP,NGAY_NHAP_CALL,SO_HOP_DONG,TINH_TRANG,NGAY_QUYET_DINH,NGAY_XAY_RA,NGAY_THONG_BAO,SO_HO_SO,MA_GDV,GIA_TRI_BH FROM CLAIM_GENERAL WHERE ID=:id";Add(cmd,"id",id);cn.Open();using(var rd=cmd.ExecuteReader())return rd.Read()?MapClaim(rd):null;}}
        public int Insert(Claim x){using(var cn=new OracleConnection(_cs))using(var cmd=cn.CreateCommand()){cmd.CommandText=@"INSERT INTO CLAIM_GENERAL(ID,MA_DON_VI,TEN_DON_VI,MA_KHU_VUC,TEN_KHU_VUC,BIEN_SO,NGAY_NHAP,NGAY_NHAP_CALL,SO_HOP_DONG,TINH_TRANG,NGAY_QUYET_DINH,NGAY_XAY_RA,NGAY_THONG_BAO,SO_HO_SO,MA_GDV,GIA_TRI_BH) VALUES(CLAIM_GENERAL_SEQ.NEXTVAL,:a,:b,:c,:d,:e,:f,:g,:h,:i,:j,:k,:l,:m,:n,:o) RETURNING ID INTO :newId";BindClaim(cmd,x);var p=new OracleParameter("newId",OracleDbType.Int32){Direction=System.Data.ParameterDirection.Output};cmd.Parameters.Add(p);cn.Open();cmd.ExecuteNonQuery();return Convert.ToInt32(p.Value.ToString());}}
        public void Update(Claim x){using(var cn=new OracleConnection(_cs))using(var cmd=cn.CreateCommand()){cmd.CommandText=@"UPDATE CLAIM_GENERAL SET MA_DON_VI=:a,TEN_DON_VI=:b,MA_KHU_VUC=:c,TEN_KHU_VUC=:d,BIEN_SO=:e,NGAY_NHAP=:f,NGAY_NHAP_CALL=:g,SO_HOP_DONG=:h,TINH_TRANG=:i,NGAY_QUYET_DINH=:j,NGAY_XAY_RA=:k,NGAY_THONG_BAO=:l,SO_HO_SO=:m,MA_GDV=:n,GIA_TRI_BH=:o WHERE ID=:id";BindClaim(cmd,x);Add(cmd,"id",x.Id);cn.Open();cmd.ExecuteNonQuery();}}
        public void Delete(int id){using(var cn=new OracleConnection(_cs))using(var cmd=cn.CreateCommand()){cmd.CommandText="DELETE FROM CLAIM_GENERAL WHERE ID=:id";Add(cmd,"id",id);cn.Open();cmd.ExecuteNonQuery();}}
        public bool ClaimNumberExists(string n,int? exceptId){using(var cn=new OracleConnection(_cs))using(var cmd=cn.CreateCommand()){cmd.CommandText="SELECT COUNT(1) FROM CLAIM_GENERAL WHERE SO_HO_SO=:n AND (:id IS NULL OR ID<>:id2)";Add(cmd,"n",n);Add(cmd,"id",exceptId);Add(cmd,"id2",exceptId);cn.Open();return Convert.ToInt32(cmd.ExecuteScalar())>0;}}
        public LossPaymentViewModel GetLossPayment(int claimId){ return new DemoClaimRepository().GetLossPayment(claimId); }
        public void SaveLossPayment(LossPaymentViewModel model){ throw new NotSupportedException("Oracle module Tổn thất/Chi trả: chạy script Database/01_create_schema.sql và triển khai mapping theo schema doanh nghiệp. Demo mode đã hoạt động đầy đủ."); }
        public QuoteViewModel GetQuote(int claimId){ return new DemoClaimRepository().GetQuote(claimId); }
        public void SaveQuote(QuoteViewModel model){ throw new NotSupportedException("Oracle module Báo giá: chạy script Database/01_create_schema.sql và triển khai mapping theo schema doanh nghiệp. Demo mode đã hoạt động đầy đủ."); }
        private static void Add(OracleCommand c,string n,object v){c.Parameters.Add(n,v??DBNull.Value);}
        private static void BindClaim(OracleCommand c,Claim x){Add(c,"a",x.ManagementUnitCode);Add(c,"b",x.ManagementUnitName);Add(c,"c",x.ManagementAreaCode);Add(c,"d",x.ManagementAreaName);Add(c,"e",x.LicensePlate);Add(c,"f",x.EntryDate);Add(c,"g",x.CallEntryDate);Add(c,"h",x.PolicyNumber);Add(c,"i",x.Status);Add(c,"j",x.DecisionDate);Add(c,"k",x.AccidentDate);Add(c,"l",x.NotificationDate);Add(c,"m",x.ClaimNumber);Add(c,"n",x.SurveyorCode);Add(c,"o",x.InsuredValue);}
        private static Claim MapClaim(OracleDataReader r){return new Claim{Id=Convert.ToInt32(r["ID"]),ManagementUnitCode=r["MA_DON_VI"].ToString(),ManagementUnitName=r["TEN_DON_VI"].ToString(),ManagementAreaCode=r["MA_KHU_VUC"].ToString(),ManagementAreaName=r["TEN_KHU_VUC"].ToString(),LicensePlate=r["BIEN_SO"].ToString(),EntryDate=Convert.ToDateTime(r["NGAY_NHAP"]),CallEntryDate=r["NGAY_NHAP_CALL"]==DBNull.Value?(DateTime?)null:Convert.ToDateTime(r["NGAY_NHAP_CALL"]),PolicyNumber=r["SO_HOP_DONG"].ToString(),Status=r["TINH_TRANG"].ToString(),DecisionDate=r["NGAY_QUYET_DINH"]==DBNull.Value?(DateTime?)null:Convert.ToDateTime(r["NGAY_QUYET_DINH"]),AccidentDate=Convert.ToDateTime(r["NGAY_XAY_RA"]),NotificationDate=Convert.ToDateTime(r["NGAY_THONG_BAO"]),ClaimNumber=r["SO_HO_SO"].ToString(),SurveyorCode=r["MA_GDV"].ToString(),InsuredValue=r["GIA_TRI_BH"]==DBNull.Value?0:Convert.ToDecimal(r["GIA_TRI_BH"])};}
    }
}
