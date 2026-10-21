using BlazorPortal.Entities;
using MailKit.Net.Smtp;
using MimeKit;

namespace BlazorPortal.Services;

public class PaymentRequestEmailService
{
	public async Task SendPaymentRequestLinkAsync(PaymentRequest request, PaymentRequestFrom from, string paymentLink)
	{
		var client = new SmtpClient();
		client.Connect(from.ServerHost, from.ServerPort, false);
		client.Authenticate(from.UserName, from.Password);

		var message = new MimeMessage();

		message.From.Add(new MailboxAddress(from.FromName, from.UserName));
		message.To.Add(new MailboxAddress(request.ClientName, request.ClientEmail));
		message.Subject = "Important: Payment Request is Due";

		message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
		{
			Text = $@"<p>Dear <b>{request.ClientName}</b>,</p>
			
			<p>Please note that we have attached the payment link for our services below. 
			Kindly if you can initiate the payment today it would be highly appreciated.</p>	

			<p>Payment Details:</p>
			
			<table style=""border:none"">
			  <tr>
				<td width=""200""><b>Requested From:</b></td>
				<td>{from.FromName}</td> 
			  </tr>
			  <tr>
				<td><b>Due By:</b></td>
				<td>{request.DueDate.ToString("dd MMMM yyyy")}</td> 
			  </tr>
			  <tr>
				<td><b>Description:</b></td>
				<td>{request.Description}</td> 
			  </tr>
			  <tr>
				<td><b>Amount:</b></td>
				<td>{request.Amount:C}</td> 
			  </tr>
			</table>

			<p>By clicking the link below, you can complete your payment:</p>
			<p><a href='{paymentLink}'>Click here to pay</a></p>"
		};

		await client.SendAsync(message);
		await client.DisconnectAsync(true);
	}
}
