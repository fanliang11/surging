   {{each Entity}}
	<tr>
                            <td class="center">
								  {{if ($index+1)%2==1 }}
                                  <span class="badge badge-grey ">${$index+1}</span>
                                  {{else}}
                                  <span class="badge   badge-success  ">${$index+1}</span>
                                  {{/if}}
                            </td>
                            <td class="center"> {{= [Host,Port].join(":")}}</td>
							  <td class="center">${UserName}</td>
                            <td class="center">${Password}</td>
                            <td class="center"> ${Db}</td>
                            <td class="center"> ${MaxSize}</td>
							  <td class="center"> ${MinSize}</td>
						     <td class="center"><div class="visible-md visible-lg hidden-sm hidden-xs action-buttons">
                    <a href="javascript:void(0)" class="grey editCacheEndpoint"><i class="icon-lightbulb  bigger-120"></i>编辑</a>
					<a href="javascript:void(0);" class="red delCacheEndpoint"><i class="icon-trash bigger-120"></i>删除</a>
                    </div></td>
                        </tr>
						  {{/each}}