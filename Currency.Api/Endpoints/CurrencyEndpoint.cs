using Currencey.Api.Mapping;
using Currencey.Api.Repository;
using Currencey.Contact;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Currencey.Api.Endpoints;

public static class CurrencyEndpoint
{

    public static void MapCurrency(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiRoute.Currency.Update, Update);
        
        app.MapGet(ApiRoute.Currency.GetAll, GetAll)
            .CacheOutput("currencyCache")
            .WithTags("currency");
    }
    
    static async Task<Results<Ok<decimal>,BadRequest>> Update([FromRoute]string id, decimal amount,[FromServices]ICurrencyRepository currencyRepository,IOutputCacheStore cacheStore, CancellationToken cancellationToken = default)
    {
        if(await currencyRepository.UpdateCurrencyAsync(id, amount, cancellationToken))
        {
            await cacheStore.EvictByTagAsync("currency",cancellationToken);
            return TypedResults.Ok(amount);
        }
        return TypedResults.BadRequest();
    }
    
    static async Task<IResult> GetAll([FromServices]ICurrencyRepository currencyRepository, CancellationToken cancellationToken = default)
    {
        var currencies = await currencyRepository.GetCurrenciesAsync(cancellationToken);
        var response = currencies.ToCurrencyResponse();
        return TypedResults.Ok(response);
    }
    
}
