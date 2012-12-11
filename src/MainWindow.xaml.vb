Imports System.Web
Imports System.Net.Http
Imports System.Net.Http.Formatting
Imports Newtonsoft.Json


Class MainWindow

    Private _apiUrl As String

    Public Sub New()

        InitializeComponent()

        If My.Settings.Environment.Equals("SIM") Then
            _apiUrl = My.Settings.SIMAPIBaseUrl
        ElseIf My.Settings.Environment.Equals("PROD") Then
            _apiUrl = My.Settings.PRODAPIBaseUrl
        End If

        browser.Source = New Uri(_apiUrl & "/authorize?client_id=" & My.Settings.APIKey & "&response_type=code&redirect_uri=" & My.Settings.RedirectUri)

    End Sub

    Private Sub browser_Navigated(sender As Object, e As NavigationEventArgs) Handles browser.Navigated

        If e.Uri IsNot Nothing Then
            Dim query = HttpUtility.ParseQueryString(e.Uri.Query)
            If query.AllKeys.Contains("code") Then

                Dim authCode = query("code")

                ' Exchange authcode with access token
                ExchangeTokenFromAuthCode(authCode)

            End If
        End If

    End Sub

    Async Sub ExchangeTokenFromAuthCode(authCode As String)

        Dim authRequest = New FormUrlEncodedContent({
                                             New KeyValuePair(Of String, String)("grant_type", "authorization_code"),
                                             New KeyValuePair(Of String, String)("client_id", My.Settings.APIKey),
                                             New KeyValuePair(Of String, String)("client_secret", My.Settings.APISecret),
                                             New KeyValuePair(Of String, String)("redirect_uri", My.Settings.RedirectUri),
                                             New KeyValuePair(Of String, String)("code", authCode)
                                         })

        Using client As New HttpClient
            Dim response = Await client.PostAsync(_apiUrl & "/security/authorize", authRequest)
            response.EnsureSuccessStatusCode()
            Dim token = JsonConvert.DeserializeObject(Of AccessToken)(response.Content.ReadAsStringAsync.Result)
            _browser.NavigateToString(<html><body>token = <%= token.access_token %></body></html>.ToString)
        End Using

    End Sub

End Class
