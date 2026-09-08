using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClaimCar.Web.Models;
using ClaimCar.Web.Repositories;
using ClaimCar.Web.Services;

class CompleteClaimTests
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static Claim NewClaim(string number){return new Claim{ManagementUnitCode="001",ManagementUnitName="Unit",ManagementAreaCode="HCM",ManagementAreaName="Area",LicensePlate="TEST",PolicyNumber="TEST",Status="Mới tiếp nhận",EntryDate=DateTime.Today,AccidentDate=DateTime.Today,NotificationDate=DateTime.Today,ClaimNumber=number,CreatedBy="test"};}
    static LossPaymentViewModel NewLoss(){return new LossPaymentViewModel{CauseCode="CAUSE",AreaCode="AREA",EventCode="EVENT",AccidentDescription="Accident",CauseDescription="Cause",ConsequenceDescription="Consequence",Coverages=new List<CoverageLine>{new CoverageLine{CoverageCode="TEST",LossAmount=100}},OtherBeneficiaries=new List<BeneficiaryLine>(),ThirdParties=new List<ThirdPartyLine>()};}
    static void Main()
    {
        var repo=new SQLiteClaimRepository();
        var number=Guid.NewGuid().ToString("N");
        var loss=NewLoss();var quote=new QuoteViewModel{ApprovalType="Duyệt giá",ActualValue=1000,Items=new List<QuoteItem>{new QuoteItem{PartName="Part",Quantity=1,PartPrice=100}}};
        var id=repo.InsertComplete(NewClaim(number),loss,quote);
        Check(repo.Get(id)!=null,"Claim missing");
        Check(repo.GetLossPayment(id).CauseCode=="CAUSE","Loss missing");
        Check(repo.GetQuote(id).Items.Count==1,"Quote missing");
        var badNumber=Guid.NewGuid().ToString("N");
        try{repo.InsertComplete(NewClaim(badNumber),NewLoss(),new QuoteViewModel{Items=new List<QuoteItem>{null}});throw new Exception("Failure was expected");}
        catch(NullReferenceException){}
        Check(!repo.ClaimNumberExists(badNumber,null),"Partial claim was committed");
        var results=new List<ValidationResult>();
        var emptyLoss=new LossPaymentViewModel();
        Check(!Validator.TryValidateObject(emptyLoss,new ValidationContext(emptyLoss),results,true),"Empty loss accepted");
        var state=new ModelStateDictionary();
        QuoteValidation.Validate(new QuoteViewModel(),null,state,"quote.");Check(!state.IsValid,"Empty quote accepted");
        var q=new QuoteViewModel{ApprovalType="Duyệt giá",ActualValue=1000,DeductibleAmount=0,ParticipationFeePercent=100,Items=new List<QuoteItem>{new QuoteItem{Proposal="Sửa chữa",Quantity=2,PartPrice=100}},ApprovedTotal=999999};
        state=new ModelStateDictionary();QuoteValidation.Validate(q,new VehiclePolicy{VehicleValue=1000,InsuredAmount=1000},state,"quote.");
        Check(state.IsValid&&q.ApprovedTotal==200,"Quote must be calculated by server");
        Console.WriteLine("PASS: complete save, rollback, required loss/quote, server quote calculation.");
    }
}
