Imports System.Web
Imports System.Net.Http
Imports Newtonsoft.Json
Imports System.ComponentModel
Imports System.Windows.Threading


Class MainWindow

    Private _apiUrl As String
    Private WithEvents _worker As BackgroundWorker

    Public Sub New()

        InitializeComponent()

        If My.Settings.Environment.Equals("SIM") Then
            _apiUrl = My.Settings.SIMAPIBaseUrl
        ElseIf My.Settings.Environment.Equals("PROD") Then
            _apiUrl = My.Settings.PRODAPIBaseUrl
        End If

        browser.Source = New Uri(_apiUrl & "/authorize?client_id=" & My.Settings.APIKey & "&response_type=code&redirect_uri=" & My.Settings.RedirectUri)

        _worker = New BackgroundWorker
        _worker.RunWorkerAsync()

    End Sub

    Private Sub browser_Navigated(sender As Object, e As NavigationEventArgs) Handles browser.Navigated

        If e.Uri IsNot Nothing Then
            Dim query = HttpUtility.ParseQueryString(e.Uri.Query)
            If query.AllKeys.Contains("code") Then

                Dim authCode = query("code")

                ' Run Async process
                _worker.RunWorkerAsync(authCode)

            End If
        End If

    End Sub

    Private Sub _worker_DoWork(sender As Object, e As DoWorkEventArgs) Handles _worker.DoWork
        ' Exchange authcode with access token
        ExchangeTokenFromAuthCode(e.Argument.ToString)
    End Sub

    Private Sub ExchangeTokenFromAuthCode(authCode As String)

        Dim authRequest = New FormUrlEncodedContent({
                                             New KeyValuePair(Of String, String)("grant_type", "authorization_code"),
                                             New KeyValuePair(Of String, String)("client_id", My.Settings.APIKey),
                                             New KeyValuePair(Of String, String)("client_secret", My.Settings.APISecret),
                                             New KeyValuePair(Of String, String)("redirect_uri", My.Settings.RedirectUri),
                                             New KeyValuePair(Of String, String)("code", authCode)
                                         })

        Using client As New HttpClient
            Dim response = client.PostAsync(_apiUrl & "/security/authorize", authRequest).Result
            response.EnsureSuccessStatusCode()
            Dim token = JsonConvert.DeserializeObject(Of AccessToken)(response.Content.ReadAsStringAsync.Result)
            Dispatcher.Invoke(DispatcherPriority.Normal,
                              New Action(Sub() browser.NavigateToString(<html><body>token = <%= token.access_token %></body></html>.ToString)))
        End Using

    End Sub

End Class
