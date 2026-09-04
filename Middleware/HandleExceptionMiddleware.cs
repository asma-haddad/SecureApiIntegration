using ExpenseAuthApi.Exceptions;

namespace ExpenseAuthApi.Middleware
{
    public class HandleExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode =
                    StatusCodes.Status404NotFound;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
        }
    }
}

