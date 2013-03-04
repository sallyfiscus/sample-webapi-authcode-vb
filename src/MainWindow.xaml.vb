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
        ElseIf My.Settings.Environment.Equals("LIVE") Then
            _apiUrl = My.Settings.LIVEAPIBaseUrl
        End If

        browser.Source = New Uri(_apiUrl & "/authorize?client_id=" & My.Settings.APIKey & "&response_type=code&redirect_uri=" & My.Settings.RedirectUri)

    End Sub

    Async Sub browser_Navigated(sender As Object, e As NavigationEventArgs) Handles browser.Navigated

        If e.Uri IsNot Nothing Then
            Dim query = HttpUtility.ParseQueryString(e.Uri.Query)
            If query.AllKeys.Contains("code") Then

                Dim authCode = query("code")

                ' Exchange authcode with access token
                Dim accessToken = Await ExchangeTokenFromAuthCode(authCode)

                ' Get Account Balances
                Dim accounts = Await GetAccountsInfo(accessToken)
                Dim balances = Await GetBalances(accounts.First.Key, accessToken)

                _browser.NavigateToString(<html><body><p>token = <%= accessToken.access_token %></p><p>account = <%= balances.First.Key %></p></body></html>.ToString)
            End If
        End If

    End Sub

    Async Function ExchangeTokenFromAuthCode(authCode As String) As Task(Of AccessToken)

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
            Return JsonConvert.DeserializeObject(Of AccessToken)(response.Content.ReadAsStringAsync.Result)
        End Using

    End Function

    Async Function GetBalances(accountNumber As Integer, accessToken As AccessToken) As Task(Of List(Of Account))
        Using client As New HttpClient
            Dim request = New HttpRequestMessage With
                          {
                              .RequestUri = New Uri(String.Format("{0}/accounts/{1}/balances", _apiUrl, accountNumber)),
                              .Method = HttpMethod.Get
                          }
            request.Headers.Add("Authorization", String.Format("Bearer {0}", accessToken.access_token))

            Dim response = Await client.SendAsync(request)
            response.EnsureSuccessStatusCode()
            Return JsonConvert.DeserializeObject(Of List(Of Account))(response.Content.ReadAsStringAsync.Result)
        End Using
    End Function

    Async Function GetAccountsInfo(accessToken As AccessToken) As Task(Of List(Of AccountInfo))
        Using client As New HttpClient
            Dim request = New HttpRequestMessage With
                          {
                              .RequestUri = New Uri(String.Format("{0}/users/{1}/accounts", _apiUrl, accessToken.userid)),
                              .Method = HttpMethod.Get
                          }
            request.Headers.Add("Authorization", String.Format("Bearer {0}", accessToken.access_token))

            Dim response = Await client.SendAsync(request)
            response.EnsureSuccessStatusCode()
            Return JsonConvert.DeserializeObject(Of List(Of AccountInfo))(response.Content.ReadAsStringAsync.Result)
        End Using
    End Function

End Class
