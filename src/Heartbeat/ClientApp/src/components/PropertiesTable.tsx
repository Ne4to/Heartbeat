export type PropertyRow = {
    title: string,
    value?: string
}

export type PropertiesTableProps = {
    rows: PropertyRow[]
}

export const PropertiesTable = (props: PropertiesTableProps) => {
    if (props.rows.length === 0)
        return (<></>)

    const rows = props.rows.map(row =>
        <tr key={row.title}>
            <td className="px-1">{row.title}</td>
            <td className="px-1">{row.value}</td>
        </tr>)

    return (
        <div>
            <table>
                <tbody>
                    {rows}
                </tbody>
            </table>
        </div>
    )
}