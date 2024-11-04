import React, { Component } from "react";

const AuthProviderHOC = (C) =>
    class AuthProvider extends Component {
        constructor(props) {
            super(props);

            this.state = {
                isAuthenticated: false,
                account: null,
            };
        }

        async componentDidMount() {
            await this.getAccount();
        }

        login = (postLoginRedirectUri, scopesToConsent) => {
            let url = "api/auth/login";

            const searchParams = new URLSearchParams({});

            if (postLoginRedirectUri) {
                searchParams.append('postLoginRedirectUri', encodeURIComponent(postLoginRedirectUri));
            }

            if (scopesToConsent) {
                searchParams.append('scopesToConsent', scopesToConsent.join(' '));
            }

            url = `${url}?${searchParams.toString()}`;

            window.location.replace(url);
        }

        logout = (postLogoutRedirectUri) => {
            this.setState({ isAuthenticated: false, account: null });

            let url = "api/auth/logout";

            const searchParams = new URLSearchParams({});

            if (postLogoutRedirectUri) {
                searchParams.append('postLogoutRedirectUri', encodeURIComponent(postLogoutRedirectUri));
            }

            url = `${url}?${searchParams.toString()}`;

            window.location.replace(url);
        }

        getAccount = async () => {
            const response = await fetch('api/auth/account');
            const data = await response.json();

            if (data.isAuthenticated) {
                this.setState({ account: data.claims });
            }

            this.setState({ isAuthenticated: data.isAuthenticated });
        }

        render() {
            return (
                <C
                    {...this.props}
                    isAuthenticated={this.state.isAuthenticated}
                    account={this.state.account}
                    login={this.login}
                    logout={this.logout}
                />
            );
        }
    };

export default AuthProviderHOC;