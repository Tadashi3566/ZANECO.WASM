using CurrieTechnologies.Razor.SweetAlert2;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Infrastructure.ApiClient;

namespace ZANECO.WASM.Client.Shared;

public static class ApiHelper
{
    //private static readonly SweetAlertService? Swal;

    public static string ErrorString = string.Empty;

    public static async Task<T?> ExecuteCallGuardedAsync<T>(
        Func<Task<T>> call,
        ISnackbar snackbar,
        CustomValidation? customValidation = null,
        string? successMessage = null)
    {
        customValidation?.ClearErrors();
        try
        {
            var result = await call();

            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                snackbar.Add(successMessage, Severity.Info);
                //await Swal.FireAsync("Success" ,successMessage, SweetAlertIcon.Success);
            }

            return result;
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            if (ex.Result.Errors is not null)
            {
                customValidation?.DisplayErrors(ex.Result.Errors);
            }
            else
            {
                snackbar.Add("Something went wrong!", Severity.Error);
                //await Swal.FireAsync("Error", "Something went wrong!", SweetAlertIcon.Error);
            }
        }
        catch (ApiException<ErrorResult> ex)
        {
            snackbar.Add(ex.Result.Exception, Severity.Error);
            //await Swal.FireAsync("Error", ex.Result.Exception, SweetAlertIcon.Error);
        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);
            //await Swal.FireAsync("Error", ex.Message, SweetAlertIcon.Error);
        }

        return default;
    }

    public static async Task<bool> ExecuteCallGuardedAsync(
        Func<Task> call,
        ISnackbar snackbar,
        CustomValidation? customValidation = null,
        string? successMessage = null)
    {
        customValidation?.ClearErrors();
        try
        {
            await call();

            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                snackbar.Add(successMessage, Severity.Success);
                //await Swal.FireAsync("Success" ,successMessage, SweetAlertIcon.Success);
            }

            return true;
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            if (ex.Result.Errors is not null)
            {
                customValidation?.DisplayErrors(ex.Result.Errors);
            }
            else
            {
                snackbar.Add("Something went wrong!", Severity.Error);
                //await Swal.FireAsync("Error", "Something went wrong!", SweetAlertIcon.Error);
            }
        }
        catch (ApiException<ErrorResult> ex)
        {
            snackbar.Add(ex.Result.Exception, Severity.Error);
            //await Swal.FireAsync("Error", ex.Result.Exception, SweetAlertIcon.Error);
        }

        return false;
    }
}