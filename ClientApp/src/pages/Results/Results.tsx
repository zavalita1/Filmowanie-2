import { AppComponentProps, Layout } from "../Layout";

const Results: React.FC<AppComponentProps> = () => {
    return (
        <div>looo</div>
    );
}

const module: React.FC<AppComponentProps> = (props) => { return <Layout><Results {...props}/></Layout>}

export default module;