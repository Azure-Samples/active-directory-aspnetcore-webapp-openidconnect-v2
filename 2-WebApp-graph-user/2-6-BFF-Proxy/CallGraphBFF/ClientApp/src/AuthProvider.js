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

        login = () => {
            window.location.replace('api/auth/login');
        }

        logout = () =>{
            this.setState({ isAuthenticated: false, account: null });
            window.location.replace('api/auth/logout');
        }

        getAccount = async() => {
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