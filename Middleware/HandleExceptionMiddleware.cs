using ExpenseAuthApi.Exceptions;

namespace ExpenseAuthApi.Middleware
{
    public class HandleExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            catch (BadRequestException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            catch (ConflictException ex)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            catch (UnAuthorizedException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
            catch (UserNotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
        }
    }
}

