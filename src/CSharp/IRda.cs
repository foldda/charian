// Copyright (c) 2022 Foldda Pty Ltd
// Licensed under the GPL License -
// https://github.com/foldda/charian/blob/main/LICENSE

using System;

namespace Charian
{
    /*
     * RDA-Serializable object implements this interface to ensure 
     * 1) its properties can be stored into an resulting RDA object,  
     * 2) used a provided RDA to restore this object's properties' values.  
     * 
     */

    public interface IRda
    {
        /// <summary>
        /// Stores properties into the RDA.
        /// </summary>
        /// <returns>An Rda instance that carries the properties of this object.</returns>
        Rda ToRda();

        /// <summary>
        /// Populate properties with the values from an RDA - effectively cloning the RDA
        /// </summary>
        /// <param name="rda">An Rda instance that carries the properties of an object to be restored.</param>
        /// <returns>An IRda instance that carries the result of the conversion. Eg, it can be the restored the object itself, or an "acknowledgement" Rda object carrying validation errors</returns>
        IRda FromRda(Rda rda);

        ///// <summary>
        ///// Construct an object of this type from the supplied (encoded) string
        ///// </summary>
        ///// <param name="stringWithLocalizedEncoding">A string that contains the encoded properties of an object using localized encoding eg char-set and time-formattings, etc.</param>
        ///// <returns>An IRda instance that carries the result of the conversion. Eg, it can be the restored the object itself, or an "error/acknowledgement" if the parsing fails</returns>
        //IRda FromString(string stringWithLocalizedEncoding);

    }
}

