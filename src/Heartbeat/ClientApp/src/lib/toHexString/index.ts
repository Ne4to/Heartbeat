export default function toHexString(value?: number): string {
    const result = value !== undefined ? value.toString(16) : ''
    return result
}
