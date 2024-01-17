import { Home } from "./components/Home";
import { InstanceTypeStatistics } from "./components/InstanceTypeStatistics"
import { ObjectInstances } from "./components/ObjectInstances"
import { ClrObject } from "./components/ClrObject"
import { Modules } from "./components/Modules"

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/instance-type-statistics',
    element: <InstanceTypeStatistics />
  },
  {
    path: '/object-instances',
    element: <ObjectInstances />
  },
  {
    path: '/clr-object',
    element: <ClrObject />
  },
  {
    path: '/modules',
    element: <Modules />
  }
];

export default AppRoutes;
