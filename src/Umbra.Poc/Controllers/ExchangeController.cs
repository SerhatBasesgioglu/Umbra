using System.Net.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Configuration;
using Umbra.Common.Dto;
using Umbra.Common.Dump;

namespace Umbra.Poc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeController(IConfiguration config) : ControllerBase
{
    private readonly string url = config["exchange:url"];
    private readonly string user = config["exchange:user"];
    private readonly string pass = config["exchange:pass"];
    private readonly string domain = config["exchange:domain"];
    private readonly string recipient = config["exchange:recipient"];

    [HttpGet]
    public async Task<string> Get()
    {
        var service = new ExchangeService(ExchangeVersion.Exchange2016) { Url = new Uri(url) };
        service.ServerCertificateValidationCallback = (
            sender,
            certificate,
            chain,
            SslPolicyErrors
        ) => true;
        service.Credentials = new WebCredentials(user, pass, domain);

        // Get unread mail
        SearchFilter unreadFilter = new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false);
        ItemView view = new(50);
        FindItemsResults<Item> results = await service.FindItems(
            WellKnownFolderName.Inbox,
            unreadFilter,
            view
        );
        await service.LoadPropertiesForItems(results.Items, PropertySet.FirstClassProperties);
        var unreadList = results
            .Items.Select(item => new { item.Subject, item.DateTimeReceived })
            .ToList();
        Console.WriteLine(unreadList.FirstOrDefault().Subject);

        //?
        Folder inbox = await Folder.Bind(service, WellKnownFolderName.Inbox);
        Console.WriteLine($"Success inbox: {inbox.DisplayName}");

        // Get folders
        FolderView view2 = new FolderView(100)
        {
            Traversal = FolderTraversal.Deep,
            PropertySet = new PropertySet(BasePropertySet.IdOnly, FolderSchema.DisplayName),
        };
        FindFoldersResults results2 = await service.FindFolders(
            WellKnownFolderName.MsgFolderRoot,
            view2
        );
        var folderData = results2
            .Folders.Select(f => new { f.DisplayName, f.Id.UniqueId })
            .ToList();

        folderData.ForEach(f => Console.WriteLine($"Folder: {f.DisplayName}"));

        // Send mail
        EmailMessage email = new(service);
        email.ToRecipients.Add(recipient);
        email.Subject = "Hello World";
        email.Body = new MessageBody("Hello world!");
        //await email.Send();

        return "yo";
    }
}
