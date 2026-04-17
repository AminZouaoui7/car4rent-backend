using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Car4rentpg.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SmtpClient CreateSmtpClient()
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = _configuration["EmailSettings:SmtpPort"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(smtpPort) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(senderPassword))
            {
                throw new Exception("Email settings are missing in appsettings.json.");
            }

            return new SmtpClient(smtpHost, int.Parse(smtpPort))
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };
        }

        private string GetSenderEmail()
        {
            var senderEmail = _configuration["EmailSettings:SenderEmail"];

            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new Exception("SenderEmail is missing in appsettings.json.");
            }

            return senderEmail;
        }

        private static string E(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string FormatMoney(double? amount)
        {
            return $"{(amount ?? 0):F2} €";
        }

        private static string FormatMoney(decimal? amount)
        {
            return $"{(amount ?? 0):F2} €";
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        private string BuildEmailLayout(
            string title,
            string subtitle,
            string statusLabel,
            string accentColor,
            string contentHtml)
        {
            var logoUrl = _configuration["AppSettings:LogoUrl"]
                          ?? "https://via.placeholder.com/180x60?text=Car4Rent";

            return $@"
<!DOCTYPE html>
<html lang='fr'>
<head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Car4Rent</title>
</head>
<body style='margin:0; padding:0; background:#eef4fb; font-family:Arial, Helvetica, sans-serif; color:#1f2937;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background:#eef4fb; margin:0; padding:24px 12px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='720' cellspacing='0' cellpadding='0' style='max-width:720px; width:100%; background:#ffffff; border-radius:24px; overflow:hidden; box-shadow:0 18px 55px rgba(6,42,75,0.12);'>

                    <tr>
                        <td style='padding:0;'>
                            <div style='background:linear-gradient(135deg,#062a4b 0%,#094371 55%,#0f5a98 100%); padding:34px 36px 30px 36px;'>
                                <div style='text-align:center;'>
                                    <img src='{logoUrl}' alt='Car4Rent' style='max-width:180px; height:auto; display:block; margin:0 auto 18px auto;' />
                                </div>

                                <div style='text-align:center; margin-bottom:14px;'>
                                    <span style='display:inline-block; padding:8px 14px; border-radius:999px; background:rgba(255,255,255,0.16); color:#ffffff; font-size:12px; font-weight:700; letter-spacing:0.4px;'>
                                        {E(statusLabel)}
                                    </span>
                                </div>

                                <h1 style='margin:0; text-align:center; font-size:30px; line-height:1.25; color:#ffffff; font-weight:800;'>
                                    {E(title)}
                                </h1>

                                <p style='margin:12px auto 0 auto; max-width:560px; text-align:center; font-size:15px; line-height:1.7; color:#dbeafe;'>
                                    {E(subtitle)}
                                </p>
                            </div>
                        </td>
                    </tr>

                    <tr>
                        <td style='padding:34px 36px 18px 36px;'>
                            {contentHtml}
                        </td>
                    </tr>

                    <tr>
                        <td style='padding:0 36px 36px 36px;'>
                            <div style='background:#f8fbff; border:1px solid #dbe7f5; border-radius:18px; padding:20px 22px;'>
                                <table role='presentation' width='100%' cellspacing='0' cellpadding='0'>
                                    <tr>
                                        <td style='font-size:14px; line-height:1.8; color:#4b5563;'>
                                            <strong style='color:#062a4b;'>Car4Rent</strong><br/>
                                            Location de voitures • Longue durée • Transferts<br/>
                                            WhatsApp : <strong>+216 53 063 000</strong> / <strong>+216 29 058 333</strong>
                                        </td>
                                        <td align='right' style='font-size:13px; color:{accentColor}; font-weight:700; vertical-align:top;'>
                                            Merci pour votre confiance
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string BuildInfoTable(string title, params (string Label, string Value)[] rows)
        {
            var sb = new StringBuilder();

            sb.Append($@"
<table role='presentation' width='100%' cellspacing='0' cellpadding='0'
       style='border-collapse:separate; border-spacing:0; margin:0 0 24px 0; background:#ffffff; border:1px solid #dbe7f5; border-radius:18px; overflow:hidden;'>
    <tr>
        <td colspan='2' style='padding:18px 22px; background:linear-gradient(180deg,#f4f8ff 0%,#eef5ff 100%); border-bottom:1px solid #dbe7f5;'>
            <div style='font-size:18px; font-weight:800; color:#062a4b;'>{E(title)}</div>
        </td>
    </tr>");

            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                var border = i == rows.Length - 1 ? "" : "border-bottom:1px solid #edf2f7;";

                sb.Append($@"
    <tr>
        <td style='padding:14px 20px; width:42%; font-size:14px; font-weight:700; color:#062a4b; background:#fbfdff; {border}'>
            {E(row.Label)}
        </td>
        <td style='padding:14px 20px; font-size:14px; color:#374151; {border}'>
            {E(row.Value)}
        </td>
    </tr>");
            }

            sb.Append("</table>");

            return sb.ToString();
        }

        private string BuildStatusBox(
            string background,
            string border,
            string titleColor,
            string title,
            string html)
        {
            return $@"
<div style='margin:0 0 24px 0; background:{background}; border:1px solid {border}; border-radius:18px; padding:20px 22px;'>
    <div style='font-size:18px; font-weight:800; color:{titleColor}; margin:0 0 10px 0;'>
        {E(title)}
    </div>
    <div style='font-size:14px; line-height:1.8; color:#374151;'>
        {html}
    </div>
</div>";
        }

        private string BuildPrimaryButton(string url, string text)
        {
            return $@"
<div style='text-align:center; margin:28px 0 30px 0;'>
    <a href='{E(url)}'
       style='display:inline-block; background:linear-gradient(135deg,#062a4b 0%,#094371 55%,#0f5a98 100%); color:#ffffff; text-decoration:none; padding:16px 30px; border-radius:14px; font-size:16px; font-weight:800; letter-spacing:0.2px; box-shadow:0 10px 24px rgba(6,42,75,0.18);'>
        {E(text)}
    </a>
</div>";
        }

        private string BuildSimpleTextBlock(string html)
        {
            return $@"
<div style='margin:0 0 22px 0; font-size:15px; line-height:1.8; color:#374151;'>
    {html}
</div>";
        }
        private string BuildContactBlock()
        {
            return @"
<div style='margin:0 0 24px 0; background:#f9fbff; border:1px solid #dbe7f5; border-radius:18px; padding:22px;'>
    <div style='font-size:18px; font-weight:800; color:#062a4b; margin-bottom:12px;'>
        Contact et confirmation
    </div>
    <div style='font-size:14px; line-height:1.8; color:#374151;'>
        Pour la confirmation et pour discuter des détails de la location, vous pouvez nous contacter directement sur WhatsApp :
        <br/><br/>
        <strong style='color:#062a4b;'>+216 53 063 000</strong><br/>
        <strong style='color:#062a4b;'>+216 29 058 333</strong>
    </div>
</div>";
        }

        private string BuildRentalConditionsBlock()
        {
            return @"
<div style='margin:0 0 24px 0; background:#fffaf0; border:1px solid #f6deb2; border-radius:18px; padding:22px;'>
    <div style='font-size:18px; font-weight:800; color:#7c4b00; margin-bottom:12px;'>
        Informations importantes
    </div>

    <div style='font-size:14px; line-height:1.8; color:#4b5563;'>
        Nous vous informons qu'une <strong>caution de 500 € minimum</strong> sera demandée comme garantie financière à la livraison du véhicule.
        Cette caution peut être déposée :
        <br/><br/>
        • en <strong>espèce</strong><br/>
        • ou par <strong>pré-autorisation bancaire</strong> sur carte de crédit <strong>VISA</strong> ou <strong>MASTERCARD</strong> uniquement
        <br/><br/>
        Cette garantie vous sera restituée à la restitution du véhicule, selon les conditions prévues.
        <br/><br/>
        <strong style='color:#b45309;'>Les cartes prépayées (MAESTRO, CIRRUS, etc.) ne sont pas acceptées pour le dépôt de garantie.</strong>
        <br/><br/>
        Merci de nous envoyer :
        <br/>
        • votre numéro de vol<br/>
        • votre heure d’arrivée estimée<br/>
        • vos pièces d’identité
    </div>
</div>";
        }

        private string BuildPaymentSummaryBox(double depositAmount, double? totalPrice)
        {
            var remaining = Math.Max((totalPrice ?? 0) - depositAmount, 0);

            return $@"
<div style='margin:0 0 24px 0; background:linear-gradient(180deg,#effaf3 0%,#eefbf3 100%); border:1px solid #cdeedd; border-radius:20px; padding:22px;'>
    <div style='font-size:18px; font-weight:800; color:#0f5132; margin-bottom:14px;'>
        Paiement de l’acompte
    </div>

    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='border-collapse:collapse;'>
        <tr>
            <td style='padding:10px 0; font-size:14px; color:#355b47; border-bottom:1px solid #d9efe3;'>Montant total de la réservation</td>
            <td align='right' style='padding:10px 0; font-size:14px; color:#0f5132; font-weight:700; border-bottom:1px solid #d9efe3;'>{FormatMoney(totalPrice)}</td>
        </tr>
        <tr>
            <td style='padding:10px 0; font-size:14px; color:#355b47; border-bottom:1px solid #d9efe3;'>Acompte à payer maintenant</td>
            <td align='right' style='padding:10px 0; font-size:16px; color:#0f5132; font-weight:800; border-bottom:1px solid #d9efe3;'>{depositAmount:F2} €</td>
        </tr>
        <tr>
            <td style='padding:10px 0; font-size:14px; color:#355b47;'>Reste estimé</td>
            <td align='right' style='padding:10px 0; font-size:14px; color:#0f5132; font-weight:700;'>{remaining:F2} €</td>
        </tr>
    </table>

    <div style='margin-top:14px; font-size:13px; line-height:1.7; color:#4b6b58;'>
        L’acompte permet de sécuriser définitivement votre réservation.
    </div>
</div>";
        }

        private string BuildReservationDetailsHtml(
            string customerFullName,
            string vehicleName,
            DateTime startDate,
            DateTime endDate,
            int? totalDays,
            double? totalPrice,
            string pickupCity,
            string returnCity)
        {
            return $@"
{BuildSimpleTextBlock($@"
Bonjour <strong>{E(customerFullName)}</strong>,<br/><br/>
Merci pour votre demande. Voici un récapitulatif complet de votre réservation.
")}

{BuildInfoTable(
    "Détails de la réservation",
    ("Nom du client", customerFullName),
    ("Véhicule", vehicleName),
    ("Date de départ", FormatDate(startDate)),
    ("Date de retour", FormatDate(endDate)),
    ("Nombre de jours", (totalDays ?? 0).ToString()),
    ("Lieu de prise en charge", pickupCity),
    ("Lieu de retour", returnCity),
    ("Montant total", FormatMoney(totalPrice))
)}";
        }

        private string BuildLongTermDetailsHtml(
            string customerFullName,
            DateTime startDate,
            int durationMonths,
            string pickupCity,
            string? vehicleName,
            string? notes,
            decimal? monthlyPrice = null,
            decimal? totalPrice = null,
            bool includeNotes = false,
            bool includePrices = false)
        {
            var rows = new List<(string Label, string Value)>
            {
                ("Nom du client", customerFullName),
                ("Date de début souhaitée", FormatDate(startDate)),
                ("Durée", $"{durationMonths} mois"),
                ("Ville de départ", pickupCity),
                ("Véhicule souhaité", string.IsNullOrWhiteSpace(vehicleName) ? "Non spécifié" : vehicleName!)
            };

            if (includeNotes)
            {
                rows.Add(("Remarques", string.IsNullOrWhiteSpace(notes) ? "Aucune remarque" : notes!));
            }

            if (includePrices)
            {
                rows.Add(("Mensualité proposée", monthlyPrice.HasValue ? FormatMoney(monthlyPrice) : "Non renseigné"));
                rows.Add(("Montant total proposé", totalPrice.HasValue ? FormatMoney(totalPrice) : "Non renseigné"));
            }

            return $@"
{BuildSimpleTextBlock($@"
Bonjour <strong>{E(customerFullName)}</strong>,<br/><br/>
Merci pour votre demande de location longue durée. Voici le récapitulatif de votre dossier.
")}

{BuildInfoTable("Détails de votre demande longue durée", rows.ToArray())}";
        }

        private string BuildTransferDetailsHtml(
            string customerFullName,
            string pickupAirport,
            string dropoffCity,
            string hotelName,
            string? hotelAddress,
            DateTime transferDate,
            int passengers,
            int luggageCount)
        {
            return $@"
{BuildSimpleTextBlock($@"
Bonjour <strong>{E(customerFullName)}</strong>,<br/><br/>
Merci pour votre demande de transfert. Voici le récapitulatif complet de votre trajet.
")}

{BuildInfoTable(
    "Détails du transfert",
    ("Nom du client", customerFullName),
    ("Aéroport de prise en charge", pickupAirport),
    ("Ville de destination", dropoffCity),
    ("Hôtel", hotelName),
    ("Adresse de l’hôtel", string.IsNullOrWhiteSpace(hotelAddress) ? "Non renseignée" : hotelAddress!),
    ("Date du transfert", FormatDate(transferDate)),
    ("Nombre de passagers", passengers.ToString()),
    ("Nombre de bagages", luggageCount.ToString())
)}";
        }

        public async Task SendBookingPendingEmailAsync(
            string customerEmail,
            string customerFullName,
            string vehicleName,
            DateTime startDate,
            DateTime endDate,
            int totalDays,
            double totalPrice,
            string pickupCity,
            string returnCity)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var details = BuildReservationDetailsHtml(
                customerFullName,
                vehicleName,
                startDate,
                endDate,
                totalDays,
                totalPrice,
                pickupCity,
                returnCity
            );

            var statusBox = BuildStatusBox(
                "#fff8e8",
                "#fde7b0",
                "#7c5a00",
                "Demande en attente",
                @"
<p style='margin:0;'>
Votre demande de réservation a bien été reçue et elle est actuellement <strong>en attente de validation</strong>.
<br/><br/>
Notre équipe va vérifier la disponibilité du véhicule et reviendra vers vous dans les plus brefs délais.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Demande de réservation reçue",
                "Votre demande a bien été enregistrée par notre équipe.",
                "Réservation en attente",
                "#c28a00",
                details + statusBox + BuildRentalConditionsBlock() + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Confirmation de votre demande de réservation",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }
        public async Task SendBookingConfirmedEmailAsync(
            string bookingId,
            string customerEmail,
            string customerFullName,
            string vehicleName,
            DateTime startDate,
            DateTime endDate,
            int? totalDays,
            double? totalPrice,
            string pickupCity,
            string returnCity)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            double depositAmount = Math.Round((totalPrice ?? 0) * 0.10, 2);

            var frontendBaseUrl = (_configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
            var paymentPagePath = _configuration["AppSettings:PaymentPagePath"] ?? "/payment";

            if (!paymentPagePath.StartsWith("/"))
            {
                paymentPagePath = "/" + paymentPagePath;
            }

            var paymentUrl =
                $"{frontendBaseUrl}{paymentPagePath}" +
                $"?bookingId={Uri.EscapeDataString(bookingId)}" +
                $"&email={Uri.EscapeDataString(customerEmail)}" +
                $"&name={Uri.EscapeDataString(customerFullName)}" +
                $"&vehicle={Uri.EscapeDataString(vehicleName)}" +
                $"&amount={depositAmount.ToString(CultureInfo.InvariantCulture)}";

            var details = BuildReservationDetailsHtml(
                customerFullName,
                vehicleName,
                startDate,
                endDate,
                totalDays,
                totalPrice,
                pickupCity,
                returnCity
            );

            var confirmationText = BuildStatusBox(
                "#eef5ff",
                "#cfe0f5",
                "#062a4b",
                "Réservation confirmée",
                $@"
<p style='margin:0;'>
Nous avons le plaisir de répondre positivement à votre demande.
<br/><br/>
La location de <strong>{E(vehicleName)}</strong> pour la période du
<strong>{FormatDate(startDate)}</strong> au <strong>{FormatDate(endDate)}</strong>
correspond à <strong>{(totalDays ?? 0)} jours</strong> pour un montant total de
<strong>{FormatMoney(totalPrice)}</strong>, sous réserve de disponibilité finale du véhicule.
</p>"
            );

            var paymentBox = BuildPaymentSummaryBox(depositAmount, totalPrice);
            var button = BuildPrimaryButton(paymentUrl, "Procéder au paiement");

            var htmlBody = BuildEmailLayout(
                "Votre réservation est confirmée",
                "Pour finaliser votre réservation, merci de régler l’acompte demandé.",
                "Réservation confirmée",
                "#0f7b4b",
                details + confirmationText + paymentBox + button + BuildRentalConditionsBlock() + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Réservation confirmée - Paiement requis",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }

        public async Task SendBookingCancelledEmailAsync(
            string customerEmail,
            string customerFullName,
            string vehicleName,
            DateTime startDate,
            DateTime endDate,
            int? totalDays,
            double? totalPrice,
            string pickupCity,
            string returnCity)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var details = BuildReservationDetailsHtml(
                customerFullName,
                vehicleName,
                startDate,
                endDate,
                totalDays,
                totalPrice,
                pickupCity,
                returnCity
            );

            var statusBox = BuildStatusBox(
                "#fff1f2",
                "#fecdd3",
                "#881337",
                "Réservation non retenue",
                @"
<p style='margin:0;'>
Nous vous informons que votre demande n’a pas pu être retenue pour le moment.
<br/><br/>
Cela peut être lié à la disponibilité du véhicule ou à l’organisation interne des réservations.
Notre équipe reste à votre disposition pour vous proposer une autre solution.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Votre demande n’a pas été retenue",
                "Nous restons disponibles pour vous proposer une alternative.",
                "Réservation refusée",
                "#b42318",
                details + statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Votre réservation n’a pas été retenue",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }

        public async Task SendDepositPaidEmailAsync(
            string customerEmail,
            string customerFullName,
            double depositAmount)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var statusBox = BuildStatusBox(
                "#eefbf3",
                "#cdeedd",
                "#0f5132",
                "Acompte reçu",
                $@"
<p style='margin:0;'>
Bonjour <strong>{E(customerFullName)}</strong>,
<br/><br/>
Nous confirmons la bonne réception de votre acompte de <strong>{depositAmount:F2} €</strong>.
Votre réservation est maintenant sécurisée et enregistrée par notre équipe.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Paiement d’acompte confirmé",
                "Votre acompte a bien été reçu.",
                "Paiement validé",
                "#0f7b4b",
                statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Confirmation de réception de votre acompte",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }

        public async Task SendFullyPaidEmailAsync(
            string customerEmail,
            string customerFullName,
            double totalPrice)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var statusBox = BuildStatusBox(
                "#eef5ff",
                "#cfe0f5",
                "#062a4b",
                "Paiement complet confirmé",
                $@"
<p style='margin:0;'>
Bonjour <strong>{E(customerFullName)}</strong>,
<br/><br/>
Nous confirmons que votre réservation est désormais <strong>entièrement réglée</strong>.
Le montant total enregistré est de <strong>{totalPrice:F2} €</strong>.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Paiement complet reçu",
                "Le règlement total de votre réservation a bien été enregistré.",
                "Paiement complet",
                "#0f7b4b",
                statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Paiement total confirmé",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }

        public async Task SendLongTermRentalPendingEmailAsync(
            string customerEmail,
            string customerFullName,
            DateTime startDate,
            int durationMonths,
            string pickupCity,
            string? vehicleName,
            string? notes)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var details = BuildLongTermDetailsHtml(
                customerFullName,
                startDate,
                durationMonths,
                pickupCity,
                vehicleName,
                notes,
                includeNotes: true,
                includePrices: false
            );

            var statusBox = BuildStatusBox(
                "#fff8e8",
                "#fde7b0",
                "#7c5a00",
                "Demande longue durée en attente",
                @"
<p style='margin:0;'>
Votre demande de location longue durée a bien été enregistrée.
Notre équipe étudie actuellement votre dossier et reviendra vers vous avec une réponse dans les plus brefs délais.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Demande longue durée reçue",
                "Votre demande a bien été enregistrée par notre équipe.",
                "Longue durée en attente",
                "#c28a00",
                details + statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Confirmation de votre demande longue durée",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }

        public async Task SendLongTermRentalQuoteEmailAsync(
            string customerEmail,
            string customerFullName,
            DateTime startDate,
            int durationMonths,
            string pickupCity,
            string? vehicleName,
            decimal? monthlyPrice,
            decimal? totalPrice)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var details = BuildLongTermDetailsHtml(
                customerFullName,
                startDate,
                durationMonths,
                pickupCity,
                vehicleName,
                notes: null,
                monthlyPrice: monthlyPrice,
                totalPrice: totalPrice,
                includeNotes: false,
                includePrices: true
            );

            var statusBox = BuildStatusBox(
                "#eef5ff",
                "#cfe0f5",
                "#062a4b",
                "Votre devis est prêt",
                @"
<p style='margin:0;'>
Notre équipe a préparé une proposition tarifaire pour votre demande de location longue durée.
Vous pouvez nous contacter pour confirmer les détails.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Votre devis longue durée",
                "Une proposition a été préparée pour votre demande.",
                "Devis disponible",
                "#0f5a98",
                details + statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Votre devis de location longue durée",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }
        public async Task SendLongTermRentalApprovedEmailAsync(
    string customerEmail,
    string customerFullName,
    DateTime startDate,
    int durationMonths,
    string pickupCity,
    string? vehicleName,
    decimal? monthlyPrice,
    decimal? totalPrice)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var details = BuildLongTermDetailsHtml(
                customerFullName,
                startDate,
                durationMonths,
                pickupCity,
                vehicleName,
                notes: null,
                monthlyPrice: monthlyPrice,
                totalPrice: totalPrice,
                includeNotes: false,
                includePrices: true
            );

            var statusBox = BuildStatusBox(
                "#eefbf3",
                "#cdeedd",
                "#0f5132",
                "Demande approuvée",
                @"
<p style='margin:0;'>
Excellente nouvelle : votre demande de location longue durée a été <strong>approuvée</strong>.
Merci de nous contacter afin de finaliser l’organisation et les derniers détails.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Location longue durée approuvée",
                "Votre demande a été validée par notre équipe.",
                "Longue durée confirmée",
                "#0f7b4b",
                details + statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Votre demande longue durée a été approuvée",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }

        public async Task SendLongTermRentalRejectedEmailAsync(
            string customerEmail,
            string customerFullName,
            DateTime startDate,
            int durationMonths,
            string pickupCity,
            string? vehicleName)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var details = BuildLongTermDetailsHtml(
                customerFullName,
                startDate,
                durationMonths,
                pickupCity,
                vehicleName,
                notes: null,
                includeNotes: false,
                includePrices: false
            );

            var statusBox = BuildStatusBox(
                "#fff1f2",
                "#fecdd3",
                "#881337",
                "Demande non retenue",
                @"
<p style='margin:0;'>
Nous vous informons que votre demande de location longue durée n’a pas pu être retenue pour le moment.
Notre équipe reste disponible pour étudier une autre formule avec vous.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Demande longue durée non retenue",
                "Nous restons disponibles pour une nouvelle demande.",
                "Longue durée refusée",
                "#b42318",
                details + statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Votre demande longue durée n’a pas été retenue",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }

        public async Task SendTransferPendingEmailAsync(
            string customerEmail,
            string customerFullName,
            string pickupAirport,
            string dropoffCity,
            string hotelName,
            string? hotelAddress,
            DateTime transferDate,
            int passengers,
            int luggageCount)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var details = BuildTransferDetailsHtml(
                customerFullName,
                pickupAirport,
                dropoffCity,
                hotelName,
                hotelAddress,
                transferDate,
                passengers,
                luggageCount
            );

            var statusBox = BuildStatusBox(
                "#fff8e8",
                "#fde7b0",
                "#7c5a00",
                "Transfert en attente",
                @"
<p style='margin:0;'>
Votre demande de transfert a bien été reçue.
Notre équipe vérifie l’organisation et vous enverra une confirmation dans les plus brefs délais.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Demande de transfert reçue",
                "Votre demande a bien été enregistrée par notre équipe.",
                "Transfert en attente",
                "#c28a00",
                details + statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Confirmation de votre demande de transfert",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }

        public async Task SendTransferConfirmedEmailAsync(
            string customerEmail,
            string customerFullName,
            string pickupAirport,
            string dropoffCity,
            string hotelName,
            string? hotelAddress,
            DateTime transferDate,
            int passengers,
            int luggageCount)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var details = BuildTransferDetailsHtml(
                customerFullName,
                pickupAirport,
                dropoffCity,
                hotelName,
                hotelAddress,
                transferDate,
                passengers,
                luggageCount
            );

            var statusBox = BuildStatusBox(
                "#eefbf3",
                "#cdeedd",
                "#0f5132",
                "Transfert confirmé",
                @"
<p style='margin:0;'>
Excellente nouvelle : votre transfert a été <strong>confirmé</strong>.
Notre équipe vous prendra en charge selon les informations transmises.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Votre transfert est confirmé",
                "Votre demande de transfert a été validée.",
                "Transfert confirmé",
                "#0f7b4b",
                details + statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Votre transfert a été confirmé",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }

        public async Task SendTransferCancelledEmailAsync(
            string customerEmail,
            string customerFullName,
            string pickupAirport,
            string dropoffCity,
            string hotelName,
            string? hotelAddress,
            DateTime transferDate,
            int passengers,
            int luggageCount)
        {
            using var client = CreateSmtpClient();
            var senderEmail = GetSenderEmail();

            var details = BuildTransferDetailsHtml(
                customerFullName,
                pickupAirport,
                dropoffCity,
                hotelName,
                hotelAddress,
                transferDate,
                passengers,
                luggageCount
            );

            var statusBox = BuildStatusBox(
                "#fff1f2",
                "#fecdd3",
                "#881337",
                "Transfert non retenu",
                @"
<p style='margin:0;'>
Nous vous informons que votre demande de transfert n’a pas pu être retenue pour le moment.
N’hésitez pas à nous contacter pour une autre proposition.
</p>"
            );

            var htmlBody = BuildEmailLayout(
                "Votre transfert n’a pas été retenu",
                "Nous restons disponibles pour toute assistance complémentaire.",
                "Transfert refusé",
                "#b42318",
                details + statusBox + BuildContactBlock()
            );

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Car4Rent"),
                Subject = "Votre demande de transfert n’a pas été retenue",
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mail.To.Add(customerEmail);
            await client.SendMailAsync(mail);
        }
    }
}