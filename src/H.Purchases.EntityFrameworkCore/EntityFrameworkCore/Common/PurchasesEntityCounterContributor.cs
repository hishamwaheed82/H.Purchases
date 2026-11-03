using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Purchases.Common;
using H.Platform.EntityAttachments;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace H.Purchases.EntityFrameworkCore.Common;

public class PurchasesEntityCounterContributor : IEntityCounterContributor, ITransientDependency
{
    private readonly IDbContextProvider<PurchasesDbContext> _db;

    public PurchasesEntityCounterContributor(IDbContextProvider<PurchasesDbContext> db)
    {
        _db = db;
    }

    public bool CanHandle(string entityCode) =>
        entityCode is PurchasesEntityCodes.Supplier
            or PurchasesEntityCodes.PurchaseInvoice
            or PurchasesEntityCodes.InvoiceLine;

    public async Task AdjustAsync(
        string entityCode,
        Guid recordId,
        int attachDelta,
        int noteDelta,
        int commentDelta,
        CancellationToken cancellationToken = default)
    {
        var db = await _db.GetDbContextAsync();
        switch (entityCode)
        {
            case PurchasesEntityCodes.Supplier:
                await db.Suppliers.Where(x => x.Id == recordId).ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.AttachCount, x => x.AttachCount + attachDelta)
                    .SetProperty(x => x.NoteCount, x => x.NoteCount + noteDelta)
                    .SetProperty(x => x.CommentCount, x => x.CommentCount + commentDelta), cancellationToken);
                break;
            case PurchasesEntityCodes.PurchaseInvoice:
                await db.Invoices.Where(x => x.Id == recordId).ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.AttachCount, x => x.AttachCount + attachDelta)
                    .SetProperty(x => x.NoteCount, x => x.NoteCount + noteDelta)
                    .SetProperty(x => x.CommentCount, x => x.CommentCount + commentDelta), cancellationToken);
                break;
            case PurchasesEntityCodes.InvoiceLine:
                await db.InvoiceLines.Where(x => x.Id == recordId).ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.AttachCount, x => x.AttachCount + attachDelta)
                    .SetProperty(x => x.NoteCount, x => x.NoteCount + noteDelta)
                    .SetProperty(x => x.CommentCount, x => x.CommentCount + commentDelta), cancellationToken);
                break;
        }
    }
}
