// import {DependencyList, useState} from "react";
//
// // export type StateWithFetchProps<T> = {
// //     loadData: () => Promise<T>,
// //     getChildren: (data: T) => JSX.Element,
// //     deps?: DependencyList
// // }
//

export const useStateWithFetch = () => {}

// // export const useStateWithFetch = ({someProp}) => {
// export const useStateWithFetch = <T>() => {
//     const [itemData, setItemData] = useState<T>();
//     const [isLoading, setIsLoading] = useState(false);
//
//     const resolveSessionData = useCallback(() => {
//         const data = database.getItemData(); // fetching data
//         setItemData(data); // setting fetched data to state
//     }, []);
//
//     useEffect(() => {
//         if (someProp) {
//             resolveSessionData(); // or may be fetch data based on props
//         }
//     }, [someProp]);
//
//     const addNewItem = useCallback(function (dataToAdd) {
//         /**
//          * write code to add new item to state
//          */
//     }, [])
//
//     const removeExistingItem = useCallback(function (dataOrIndexToRemove) {
//         /**
//          * write code to remove existing item from state
//          */
//     }, [])
//
//     return {
//         itemData,
//         addNewItem,
//         removeExistingItem,
//     }
// }