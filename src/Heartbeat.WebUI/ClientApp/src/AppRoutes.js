import { Home } from "./components/Home";
import { HeapDumpStat } from "./components/HeapDumpStat"
import { ObjectInstances } from "./components/ObjectInstances"
import { ClrObject } from "./components/ClrObject"
import { Modules } from "./components/Modules"

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/heap-dump-stat',
    element: <HeapDumpStat />
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
