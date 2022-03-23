function getTokenFromCache(scopes) {
    const [account] = msalInstance.getAllAccounts();
    return msalInstance.acquireTokenSilent({
        account,
        scopes
    })
    .catch(error => {
        if (error instanceof msal.InteractionRequiredAuthError) {
            return msalInstance.acquireTokenPopup({
                account,
                scopes
            });
        }
    });
};

const profileDiv = document.getElementById("profile-div");

async function loadUserData() {
    getTokenFromCache(['user.read'])
        .then(response => {
            callMSGraph('https://graph.microsoft.com/v1.0/me/messages', response.accessToken)
                .then(data => {
                    console.log(data);
                    console.log('Displaying Emails for the signed in user.');
                    const tabList = document.getElementById("list-tab");
                    tabList.innerHTML = ''; // clear tabList at each readMail call
                    const tabContent = document.getElementById("nav-tabContent");
                    tabContent.innerHTML = '';

                    data.value.map((d, i) => {
                        // Keeping it simple
                        if (i < 10) {
                            const listItem = document.createElement("a");
                            listItem.setAttribute("class", "list-group-item list-group-item-action")
                            listItem.setAttribute("id", "list" + i + "list")
                            listItem.setAttribute("data-toggle", "list")
                            listItem.setAttribute("href", "#list" + i)
                            listItem.setAttribute("role", "tab")
                            listItem.setAttribute("aria-controls", i)
                            listItem.innerHTML = d.subject;
                            tabList.appendChild(listItem)
                            const contentItem = document.createElement("div");
                            contentItem.setAttribute("class", "list-group-item list-group-item-action");
                            contentItem.setAttribute("id", "list" + i)
                            contentItem.setAttribute("role", "tabpanel")
                            contentItem.setAttribute("aria-labelledby", "list" + i + "list")
                            contentItem.innerHTML = "<strong>from: " + d.from.emailAddress.address + "</strong>";
                            tabContent.appendChild(contentItem);

                        }
                    });
                })
        }).catch(error => {
            console.log(error);
        });
}

async function loadUserEmails() {
    getTokenFromCache(['user.read'])
        .then(response => {
            callMSGraph('https://graph.microsoft.com/v1.0/me/', response.accessToken)
                .then(data => {
                    console.log(data);
                    profileDiv.innerHTML = '';
                    const title = document.createElement('p');
                    title.innerHTML = "<strong>Title: </strong>" + (data.jobTitle || 'Not set');
                    const email = document.createElement('p');
                    email.innerHTML = "<strong>Mail: </strong>" + (data.mail || 'Not set');
                    const phone = document.createElement('p');
                    phone.innerHTML = "<strong>Phone: </strong>" + (data.businessPhones[0] || 'Not set');
                    const address = document.createElement('p');
                    address.innerHTML = "<strong>Location: </strong>" + (data.officeLocation || 'Not set');
                    profileDiv.appendChild(title);
                    profileDiv.appendChild(email);
                    profileDiv.appendChild(phone);
                    profileDiv.appendChild(address);
                })
        }).catch(error => {
            console.log(error);
        });
}

async function updateUI() {
    await Promise.all([
        loadUserData(),
        loadUserEmails()
    ]);
}

function callMSGraph(endpoint, token) {
    const headers = new Headers();
    const bearer = `Bearer ${token}`;
    headers.append("Authorization", bearer);

    const options = {
        method: "GET",
        headers: headers
    };

    console.log('request made to Graph API at: ' + new Date().toString());

    return fetch(endpoint, options)
        .then(async response => {
            return await response.json()
        })
        .catch(error => console.log(error))
}

updateUI();