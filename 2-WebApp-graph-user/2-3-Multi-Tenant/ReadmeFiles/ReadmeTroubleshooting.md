## Troubleshooting

<details>
 <summary>Expand the section</summary>

 ### Error AADSTS650051

If you are receiving the following error message, you might need to **delete older service principals of this application**. Please [delete the existing [service principal](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals) from the **enterprise app** blade of the tenant before re-creating this application]. Click on the **Enterprise Applications** blade in the portal, locate this application `WebApp_MultiTenant_v2`, navigate to its **properties** and click **Delete** to delete the service principal.

> OpenIdConnectProtocolException: Message contains error: 'invalid_client', error_description: 'AADSTS650051: Application '{applicationId}' is requesting permissions that are either invalid or out of date.

If you had provisioned a service principal of this app in the past and created a new one, the tenants that had signed-in in the app might still have the previous service principal registered causing a conflict with the new one. The solution for the conflict is to delete the older service principal from each tenant in the **Enterprise Application** menu.

### Error `The provided request must include a 'response_type' input parameter`

If you try to sign-in with a Microsoft account (MSA), such as hotmail.com, outlook.com, and msn.com, you'd receive this error during admin consent because MSA is not supported at the `/common` endpoint which this sample is using to obtain the admin consent.
Please use an admin account with from the Azure AD tenant for this purpose.

 </details>