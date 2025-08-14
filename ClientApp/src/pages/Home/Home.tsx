import { useGetUserQuery } from '../../store/apis/User/userApi';
import Spinner from '../../components/Spinner';

const Home: React.FC = () => {
    const {data, error, isLoading } = useGetUserQuery();

  return (
    <>
    {
        isLoading 
        ? <Spinner /> 
        : error === undefined ? <Login /> : <div>TODO</div>
    }
    </>
  )
}

const Login: React.FC = () => {
    return (
        <div role="tablist" className="tabs tabs-border">
  <a role="tab" className="tab">Tab 1</a>
  <a role="tab" className="tab tab-active">Tab 2</a>
  <a role="tab" className="tab">Tab 3</a>
</div>
    )
}

export default Home