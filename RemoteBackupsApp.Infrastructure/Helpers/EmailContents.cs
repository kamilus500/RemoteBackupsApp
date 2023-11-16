namespace RemoteBackupsApp.Infrastructure.Helpers
{
    public static class EmailContents
    {
        public static string ConfirmationMessage = @"<!DOCTYPE html>
                            <html lang=""pl"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Pomyślna Rejestracja</title>
                                <style>
                                    body {
                                        font-family: 'Arial', sans-serif;
                                        background-color: #f4f4f4;
                                        margin: 0;
                                        padding: 0;
                                        display: flex;
                                        align-items: center;
                                        justify-content: center;
                                        height: 100vh;
                                    }

                                    .container {
                                        max-width: 400px;
                                        background-color: #fff;
                                        padding: 20px;
                                        border-radius: 8px;
                                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                        text-align: center;
                                    }

                                    h1 {
                                        color: #333;
                                    }

                                    p {
                                        color: #666;
                                        margin-bottom: 20px;
                                    }

                                    .button {
                                        display: inline-block;
                                        padding: 10px 20px;
                                        background-color: #4caf50;
                                        color: #fff;
                                        text-decoration: none;
                                        border-radius: 5px;
                                    }
                                </style>
                            </head>
                            <body>
                                <div class=""container"">
                                    <h1>Pomyślna Rejestracja</h1>
                                    <p>Dziękujemy za rejestrację! Twoje konto zostało pomyślnie utworzone.</p>
                                    <p>Zaloguj się teraz, aby uzyskać dostęp do wszystkich funkcji.</p>
                                    <a href=""http://localhost:8080"" class=""button"">Zaloguj się</a>
                                </div>
                            </body>
                            </html>";
    }
}
