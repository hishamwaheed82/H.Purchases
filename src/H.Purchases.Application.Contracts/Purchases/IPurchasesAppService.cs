using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.Purchases.Purchases;

public interface IPurchasesAppService : IApplicationService
{
    Task<PurchasesBootstrapDto> GetBootstrapAsync();

    Task<SupplierDto> CreateSupplierAsync(CreateUpdateSupplierDto input);
    Task<SupplierDto> UpdateSupplierAsync(Guid id, CreateUpdateSupplierDto input);

    Task<PurchaseInvoiceDto> CreateInvoiceAsync(CreatePurchaseInvoiceDto input);
    Task<PurchaseInvoiceDto> PostInvoiceAsync(Guid id);
    Task<PurchaseInvoiceDto> CancelInvoiceAsync(Guid id);
}
