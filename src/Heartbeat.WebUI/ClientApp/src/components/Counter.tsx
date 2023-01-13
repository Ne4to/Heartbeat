import {Component} from 'react';

type CounterProps = {
    initialCount: number;
}

type CounterState = {
    currentCount: number;
}

export class Counter extends Component<CounterProps, CounterState> {
    // static displayName = Counter.name;
    // state: CounterState = { currentCount: 0 };

    constructor(props: CounterProps) {
        super(props);
        this.state = {
            currentCount: props.initialCount
        };
    }

    incrementCounter = () => {
        this.setState((state) => {
            return {
                currentCount: state.currentCount + 1
            }
        });
    }

    render() {
        return (
            <div>
                <h1>Counter</h1>

                <p>This is a simple example of a React component.</p>

                <p aria-live="polite">Current count: <strong>{this.state.currentCount}</strong></p>

                <button className="btn btn-primary" onClick={this.incrementCounter}>Increment</button>
            </div>
        );
    }
}
