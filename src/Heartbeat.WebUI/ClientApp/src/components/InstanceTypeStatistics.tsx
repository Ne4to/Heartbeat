import React, {Component} from 'react';

type Size = {
    bytes: number;
}

type TypeStatistics = {
    instanceCount: number;
    totalSize: Size;
    typeName: string;
}

type InstanceTypeStatisticsState = {
    loading: boolean;
    statistics: TypeStatistics[];
}

export class InstanceTypeStatistics extends Component<{}, InstanceTypeStatisticsState> {
    constructor(props: {}) {
        super(props);
        this.state = {
            loading: true,
            statistics: []
        };
    }

    async componentDidMount() {
        await this.populateStatistics();
    }

    static renderStatisticsTable(statistics: TypeStatistics[]) {
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
                    <tr key={s.typeName}>
                        <td className="text-end">{s.instanceCount}</td>
                        <td className="text-end">{s.totalSize.bytes}</td>
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
            : InstanceTypeStatistics.renderStatisticsTable(this.state.statistics);

        return (
            <div>
                <h1 id="tableLabel">Instance type statistics</h1>
                {contents}
            </div>
        );
    }

    async populateStatistics() {
        const response = await fetch('api/dump/type-statistics');
        const data : TypeStatistics[] = await response.json();
        this.setState(
            {
                statistics: data,
                loading: false
            });
    }
}