import { SegmentsGrid } from "./SegmentsGrid";

export const Home = () => {
  return (
    <div>
      <ul>
        <li><a href='/modules'>Modules</a></li>
        <li><a href='/heap-dump-stat'>Heap dump stat</a></li>
      </ul>

      <SegmentsGrid />
    </div>
  );
}
