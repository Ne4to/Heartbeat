export default function toHexAddress(value?: number): string {
    const result = value ? value.toString(16).padStart(16, '0') : ''
    return result
}
