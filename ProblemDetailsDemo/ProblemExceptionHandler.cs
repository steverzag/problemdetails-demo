using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ProblemDetailsDemo
{
	[Serializable]
	public class ProblemException : Exception
	{
		public string Error { get; set; }
		public string Message { get; set; }

		public ProblemException(string error, string message) : base(message)
		{
			Error = error;
			Message = message;
		}
	}
	public class ProblemExceptionHandler : IExceptionHandler
	{

		private readonly IProblemDetailsService _problemDetailsService;

		public ProblemExceptionHandler(IProblemDetailsService problemDetailsService)
		{
			_problemDetailsService = problemDetailsService;
		}

		public async ValueTask<bool> TryHandleAsync(
			HttpContext httpContext,
			Exception exception,
			CancellationToken cancellationToken)
		{
			if (exception is not ProblemException problemException)
			{
				return true;
			}

			var problemDetails = new ProblemDetails
			{
				Status = StatusCodes.Status400BadRequest,
				Title = problemException.Error,
				Detail = problemException.Message,
				Type = "Bad Request"
			};

			httpContext.Response.StatusCode = problemDetails.Status.Value;

			return await _problemDetailsService.TryWriteAsync(
				new ProblemDetailsContext
				{
					HttpContext = httpContext,
					ProblemDetails = problemDetails
				});
		}
	}
}
