import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import { Home } from "./components/Home";
import { InstanceTypeStatistics } from "./components/InstanceTypeStatistics"

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/counter',
    element: <Counter initialCount={42} />
  },
  {
    path: '/fetch-data',
    element: <FetchData />
  },
  {
    path: '/instance-type-statistics',
    element: <InstanceTypeStatistics />
  }
];

export default AppRoutes;
