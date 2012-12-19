# sample-webapi-authcode-vb

This sample application uses VB.NET (this assumes Visual Studio 2012 and .NET 4.5) to authenticate with the TradeStation API via an OAuth 2 Authorizatin Code Grant Type. The user will be directed to TradeStation's login page to capture credentails. After a successful login, an auth code is return and is then exchanged for an access token which will be used for subsequent WebAPI calls.

## Configuration
Modify the following fields in the App.config with your appropriate values:

    <applicationSettings>
      <SampleWebapiAuthCodeVB.MySettings>
        <setting name="APIKey" serializeAs="String">
          <value>your key goes here</value>
        </setting>
        <setting name="APISecret" serializeAs="String">
          <value>your secret goes here</value>
        </setting>
        <setting name="RedirectUri" serializeAs="String">
          <value>your redirect URI goes here</value>
        </setting>
        <setting name="Environment" serializeAs="String">
          <value>SIM</value> ' Can be "SIM" for simulated trading or "PROD" for live trading
        </setting>
      </SampleWebapiAuthCodeVB.MySettings>
    </applicationSettings>

## Build Instructions
* Download and Extract the zip or clone this repo
* Open Visual Studio and Enable NuGet Package Restore
* Build and Run

## Troubleshooting
If there are any problems, open an [issue](sample-webapi-authcode-vb/issues) and we'll take a look! You can also give us feedback by e-mailing webapi@tradestation.com.
