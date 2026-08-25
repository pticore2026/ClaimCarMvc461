using System.Collections.Generic;
using ClaimCar.Web.Models;
namespace ClaimCar.Web.Repositories
{
    public interface IClaimRepository
    {
        IList<Claim> Search(string keyword, string status);
        Claim Get(int id);
        int Insert(Claim claim);
        void Update(Claim claim);
        void Delete(int id);
        bool ClaimNumberExists(string claimNumber, int? exceptId);
        LossPaymentViewModel GetLossPayment(int claimId);
        void SaveLossPayment(LossPaymentViewModel model);
        QuoteViewModel GetQuote(int claimId);
        void SaveQuote(QuoteViewModel model);
    }
}
