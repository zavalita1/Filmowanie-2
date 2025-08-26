import { Layout, AppComponentProps } from "../Layout";
//import { useAppSelector } from "../../hooks/redux";
import { useGetCurrentVotingQuery } from '../../store/apis/Voting/votingApi';
import { useNavigate } from "react-router";
import React, { useEffect, useState } from "react";
import { VotingStatus } from "../../consts/votingStatus";
import { Skeleton } from "../../components/ui/skeleton";
import { MovieCard } from "../../components/ui/MovieCard";
import { Button } from "../../components/ui/button";


const MoviesList: React.FC<AppComponentProps> = (props) => {
  const navigate = useNavigate();
  useEffect(() => {
    if (!props.userData)
      navigate('/');

    if (props.votingStatus !== VotingStatus.Voting)
      navigate('/');

  }, [props.userData, props.votingStatus]);
  const [displayMode, setDisplayMode] = useState<'Cards' | 'Carousel'>('Cards');

  const { data, error, isLoading } = useGetCurrentVotingQuery();

  if (isLoading)
    return (
    <div className="flex flex-col space-y-3">
      <Skeleton className="h-[125px] w-[250px] rounded-xl" />
      <div className="space-y-2">
        <Skeleton className="h-4 w-[250px]" />
        <Skeleton className="h-4 w-[200px]" />
      </div>
    </div>
    )

  if (error)
  {
    // TODO
  }

  return (
    <>
    <div className="mt-10 ml-auto mr-25">
    <Button onClick={() => displayMode === 'Carousel' ? setDisplayMode('Cards') : setDisplayMode('Carousel')}>Set to carousel!</Button> 
    </div>
    <div className="flex flex-row flex-wrap justify-center mt-10">
      { data!.map(d => <MovieCard {...props} movie={d}></MovieCard>)}
    </div>
    </>
  );
}


const moviesList: React.FC<AppComponentProps> = (props) => { return <Layout><MoviesList {...props}/></Layout>}

export default moviesList;