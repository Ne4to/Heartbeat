import prettyBytes from "pretty-bytes";

export default function toSizeString(value?: number): string {
    return value !== undefined ? prettyBytes(value, {binary: true}) : ''
}
