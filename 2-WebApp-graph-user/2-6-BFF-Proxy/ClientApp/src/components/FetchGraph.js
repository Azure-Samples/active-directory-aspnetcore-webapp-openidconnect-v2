import React, { Component } from 'react';

export class FetchGraph extends Component {
  static displayName = FetchGraph.name;

  constructor(props) {
    super(props);
    this.state = { profile: [], loading: true };
  }

  async componentDidMount() {
    await this.getGraphProfile();
  }

  renderGraphProfileTable = (profile) => {
    return (
      <table className='table table-striped' aria-labelledby="tableLabel" style={{ wordBreak: 'break-all' }}>
        <thead>
          <tr>
            <th>Key</th>
            <th>Value</th>
          </tr>
        </thead>
        <tbody>
          {Object.entries(profile).map((entry, index) =>
            <tr key={index}>
              <td>{entry[0]}</td>
              <td>{entry[1]}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }


  getGraphProfile = async () => {
    try {
      const response = await fetch('api/profile');

      if (response.ok) {
        const data = await response.json();
        this.setState({ profile: data, loading: false });
      } else if (response.status === 401) {
        this.props.login(window.location.href);
      }
    } catch (error) {
      console.log(error);
    }
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : this.renderGraphProfileTable(this.state.profile);

    return (
      <div>
        <h1 id="tableLabel">Graph Profile</h1>
        <p>This component demonstrates fetching data from the server.</p>
        {contents}
      </div>
    );
  }

}
