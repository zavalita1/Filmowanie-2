import { AppComponentProps, Layout } from "../Layout";

const About: React.FC<AppComponentProps> = props => {
    return (
        <div>Ain't nothin here.</div>
    )
}

const wrappedHome: React.FC<AppComponentProps> = (props) => { return <Layout><About {...props}/></Layout>}

export default wrappedHome;