import React, { Component } from 'react';

export class InstanceTypeStatistics extends Component {
  constructor(props) {
    super(props);
    this.state = { statitstics: [], loading: true };
  }

  componentDidMount() {
    this.populateStatistics();
  }

  static renderStatisticsTable(statistics) {
    return (
      <table className="table table-sm table-striped" aria-labelledby="tableLabel">
        <thead>
          <tr>
            <th>Count</th>
            <th>Total size</th>
            <th>Type</th>
          </tr>
        </thead>
        <tbody>
          {statistics.map(s =>
            <tr>
              <td class="text-end">{s.instanceCount}</td>
              <td class="text-end">{s.totalSize.bytes}</td>
              <td>{s.typeName}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : InstanceTypeStatistics.renderStatisticsTable(this.state.statitstics);

    return (
      <div>
        <h1 id="tableLabel">Instance type statistics</h1>
        {contents}
      </div>
    );
  }

  async populateStatistics() {
    const response = await fetch('dump/type-statistics');
    const data = await response.json();
    this.setState({ statitstics: data, loading: false });
  }
}