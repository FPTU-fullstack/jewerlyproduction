﻿using JewelryProduction.DTO;
using JewelryProduction.DTO.BasicDTO;

namespace JewelryProduction.Interface
{
    public interface ISaleStaffService
    {
        Task<bool> SendForApprovalAsync(string customizeRequestId, double goldWeight, string senderId);
        Task<decimal> CalculateProductCost(string CustomizeRequestId);
        Task<bool> UpdateCustomerRequestQuotation(string customerRequestId, decimal newQuotation, string newQuotationDes, string senderId);
        decimal GetDeposit(decimal productCost);
        Task<List<SaleStaffWithCountDTO>> GetStaffs();
    }
}
