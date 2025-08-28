using Microsoft.AspNetCore.Components;
using Retail.UI.Models;

namespace Retail.UI.Components.Pages;

public partial class ServiceManagement
{
    [Inject]
    public ILogger<ServiceManagement> Logger { get; set; } = default!;

    // This page will use the ServiceManager component which handles all the service logic
}
