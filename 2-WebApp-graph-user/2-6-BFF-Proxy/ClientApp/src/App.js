import React, { Component } from 'react';
import { Route, Routes } from 'react-router-dom';
import AuthProviderHOC from './AuthProvider';
import { Layout } from './components/Layout';
import { FetchGraph } from "./components/FetchGraph";
import { Home } from './components/Home';

import './custom.css';

class App extends Component {
  static displayName = App.name;

  render() {
    return (
      <Layout {...this.props}>
        <Routes>
          <Route path="fetch-graph" element={<FetchGraph {...this.props} />} />
          <Route path="/" element={<Home {...this.props} />} />
        </Routes>
      </Layout>
    );
  }
}

export default AuthProviderHOC(App);