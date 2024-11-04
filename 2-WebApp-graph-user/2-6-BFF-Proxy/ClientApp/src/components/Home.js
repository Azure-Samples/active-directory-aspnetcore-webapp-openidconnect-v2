import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

  renderAccountTable = (account) => {
    return (
      <table className='table table-striped' aria-labelledby="tableLabel" style={{ wordBreak: 'break-all' }}>
        <thead>
          <tr>
            <th>Claim</th>
            <th>Value</th>
          </tr>
        </thead>
        <tbody>
          {account.map((entry, index) =>
            <tr key={`table-data-${index}`}>
              <td>{entry.type}</td>
              <td>{entry.value}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }

  render() {
    return (
      <div>
        {this.props.isAuthenticated ?
          <>
            <h1 id="tableLabel">You are signed in!</h1>
            <p>Welcome {this.props.account.find(c => c.type === "name").value}</p>
            {this.renderAccountTable(this.props.account)}
          </>
          :
          <>
            <h1>Hello, world!</h1>
            <p>Welcome to your new single-page application, built with:</p>
            <ul>
              <li><a href='https://get.asp.net/'>ASP.NET Core</a> and <a href='https://msdn.microsoft.com/en-us/library/67ef8sbd.aspx'>C#</a> for cross-platform server-side code</li>
              <li><a href='https://facebook.github.io/react/'>React</a> for client-side code</li>
              <li><a href='http://getbootstrap.com/'>Bootstrap</a> for layout and styling</li>
            </ul>
            <p>To help you get started, we have also set up:</p>
            <ul>
              <li><strong>Client-side navigation</strong>. For example, click <em>Profile</em> after sign-in, then <em>Back</em> to return here.</li>
              <li><strong>Development server integration</strong>. In development mode, the development server from <code>create-react-app</code> runs in the background automatically, so your client-side resources are dynamically built on demand and the page refreshes when you modify any file.</li>
              <li><strong>Efficient production builds</strong>. In production mode, development-time features are disabled, and your <code>dotnet publish</code> configuration produces minified, efficiently bundled JavaScript files.</li>
            </ul>
            <p>The <code>ClientApp</code> subdirectory is a standard React application based on the <code>create-react-app</code> template. If you open a command prompt in that directory, you can run <code>npm</code> commands such as <code>npm test</code> or <code>npm install</code>.</p>
          </>
        }
      </div>
    );
  }
}
