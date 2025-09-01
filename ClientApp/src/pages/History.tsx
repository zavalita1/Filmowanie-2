import { AppComponentProps, Layout } from "./Layout";

const History: React.FC<AppComponentProps> = props => {
    return (
        <div>Ain't nothin here.</div>
    )
}

const wrappedHistory: React.FC<AppComponentProps> = (props) => { return <Layout><History {...props}/></Layout>}

export default wrappedHistory;