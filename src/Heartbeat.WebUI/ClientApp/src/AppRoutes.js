import { Home } from "./components/Home";
import { InstanceTypeStatistics } from "./components/InstanceTypeStatistics"

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/instance-type-statistics',
    element: <InstanceTypeStatistics />
  }
];

export default AppRoutes;
