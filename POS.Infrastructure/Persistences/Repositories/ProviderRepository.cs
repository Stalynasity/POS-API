﻿using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Commons.Bases.Request;
using POS.Infrastructure.Commons.Bases.Response;
using POS.Infrastructure.Persistences.Contexts;
using POS.Infrastructure.Persistences.Interfaces;

namespace POS.Infrastructure.Persistences.Repositories
{
    public class ProviderRepository : GenericRepository<Provider>, IProviderRepository
    {
        public ProviderRepository(POSContext context) : base(context) { }


        public async Task<BaseEntityResponse<Provider>> ListProviders(BaseFiltersRequest filters)
        {
            var response = new BaseEntityResponse<Provider>();

            var providers = GetEntityQuery(x => x.AuditDeleteUser == null && x.AuditDeleteDate == null)
                .Include(x => x.DocumentType) //aqui hacemos relacion para traer datos de FK q este en la entidad, solo va a soportar la relacion no la incluye, mapp
                .AsNoTracking();

            if (filters.NumFilter is not null && !string.IsNullOrEmpty(filters.TextFilter))
            {
                switch (filters.NumFilter)
                {
                    // con esto se filtra con un dato que yo le este enviando desde mi request
                    case 1:
                        providers = providers.Where(x => x.Name.Contains(filters.TextFilter));
                        break;
                    case 2:
                        providers = providers.Where(x => x.Email.Contains(filters.TextFilter));
                        break;
                    case 3:
                        providers = providers.Where(x => x.DocumentNumber.Contains(filters.TextFilter));
                        break;
                }
            }

            if (filters.StateFilter is not null)
            {
                providers = providers.Where(x => x.State.Equals(filters.StateFilter));
            }


            if (!string.IsNullOrEmpty(filters.StartDate) && !string.IsNullOrEmpty(filters.EndDate))
            {
                providers = providers.Where(x => x.AuditCreateDate >= Convert.ToDateTime(filters.StartDate)
                                                && x.AuditCreateDate <= Convert.ToDateTime(filters.EndDate).AddDays(1));
            }

            if (filters.Sort is null) filters.Sort = "Id";

            response.TotalRecords = await providers.CountAsync();
            response.Items = await Ordering(filters, providers, !(bool)filters.Download!).ToListAsync();
            return response;
        }

    }
}
