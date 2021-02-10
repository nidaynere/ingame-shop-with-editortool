# payments & shop
Payments & Shop is a tool you can design a shop with products & bundles for unlimited apps with different templates.
Use Payments tool to create a app on Payments API & template for your app by using Payments tool.

You can register your functions to ShopManager.OnPurchaseItem action, it will return a ShopItem object which contains product ids, price, counts etc. when user want to purchase something.

Payments API is made for developers. Run it in your local system or private cloud (it's a go service), create your app and template. When you pull it, you will see your PaymentsTemplate in your Resources folder, your app will use the template in your Resources folder on realtime.

******** 
API has no security. All requests is open for everyone!! Keep it private within your team!

[Known Issues]
- Prices can have multiple currencies currently, but Shop is using US Dollars only (for example on Sorting by price.)
	In future, PaymentsAppSettings contains users currency, and shop can be managed with that currency.

# restful api
Tool is already setup and using those functions already.
But anyway, a little information about the API =>
This is a tiny service which has 5 different calls.
There is also an Insomnia (Restful Client) template so you can import it on Insomnia,
and use the API functions from there.

GetApp (GET) => http://localhost:9001/GetApp/{appid}
SetApp (POST) => http://localhost:9001/GetApp/{appid}
SetTemplate (POST) => http://localhost:9001/GetApp/{appid}/{templateid}
GetTemplate (GET) => http://localhost:9001/GetApp/{appid}/{templateid}
RemoveTemplate (GET) => http://localhost:9001/RemoveTemplate/{appid}/{templateid}
